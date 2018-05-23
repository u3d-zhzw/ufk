#include <algorithm>

#include "NetWork.h"

#include "pb/Person.pb.h"

static const char MESSAGE[] = "Hello, World!\n";

std::shared_ptr<Session> NetWork::Connect(string ip, int port, NetStatueDef cbStatus, NetReceiveDef cbRecv)
{
    struct bufferevent* bev = bufferevent_socket_new(this->m_base, -1, BEV_OPT_CLOSE_ON_FREE);
    if (bev == NULL)
    {
        fprintf(stderr,"connect %s:%d failed, bufferevent_socket_new error, error:%d:%s", ip.c_str(), port, errno, strerror(errno));
        return NULL;
    }
    
    int ret = bufferevent_socket_connect_hostname(bev, NULL, AF_INET, ip.c_str(), port);
    if (ret)
    {
        fprintf(stderr,"connect %s:%d failed, bufferevent_socket_connect_hostname error:%d:%s", ip.c_str(), port, errno, strerror(errno));
        bufferevent_free(bev);
        return NULL;
    }

    this->m_cbNetStatus = cbStatus;
    this->m_cbRecv = cbRecv;

    std::shared_ptr<Session> session = Session::MakeSession();
    this->BindSession(session, bev);

    bufferevent_setcb(bev, conn_readcb, NULL, conn_eventcb, this);
    //bufferevent_setwatermark(ep->GetBufferEvent(), EV_READ, DEF_STPKG_FIRST_WATERMARK, 0);
    bufferevent_enable(bev, EV_READ|EV_WRITE);

    return 0;
}

void NetWork::Listen(int port, NetStatueDef cbStatus, NetReceiveDef cbRecv)
{
    this->m_cbNetStatus = cbStatus;
    this->m_cbRecv = cbRecv;

    struct sockaddr_in sin;
    memset(&sin, 0, sizeof(sin));
    sin.sin_family = AF_INET;
    sin.sin_port = htons(port);

    this->m_listener = evconnlistener_new_bind(this->m_base, listener_cb, (void *)this,
            LEV_OPT_REUSEABLE|LEV_OPT_CLOSE_ON_FREE, -1,
            (struct sockaddr*)&sin,
            sizeof(sin));

    if (!this->m_listener)
    {
        fprintf(stderr, "Could not create a listener!\n");
        return ;
    }
}

bool NetWork::Start()
{
    this->m_base = event_base_new();
    if (!this->m_base) {
        fprintf(stderr, "Could not initialize libevent!\n");
        return false;
    }

    return true;
}

void NetWork::Stop()
{
    evconnlistener_free(this->m_listener);
    if (this->m_base != NULL)
    {
        event_base_free(this->m_base);
    }
}

void NetWork::Loop()
{
    event_base_loop(this->m_base, EVLOOP_NONBLOCK);
}

void NetWork::Send(std::shared_ptr<Session> session, unsigned short id, ::google::protobuf::MessageLite* msg)
{
    auto itr = this->m_mapSession.find(session);
    if (itr == this->m_mapSession.end())
    {
        return;
    }

    bufferevent* bev = this->m_mapSession[session];
    if (bev == NULL)
    {
        return ;
    }
    
    size_t size = msg->ByteSizeLong();
    const unsigned short HEAD_SIZE = 4;
    unsigned short totalSize = HEAD_SIZE + size;

    // todo: check crc
    void* buff = malloc(totalSize);

    *((unsigned short*)buff) = id;
    *((unsigned short*)(buff + 2)) = totalSize;
    msg->SerializeToArray(buff + HEAD_SIZE, size);

    struct evbuffer* outbuffer = bufferevent_get_output(bev);
    if(evbuffer_add_reference(outbuffer, buff, totalSize , NULL, NULL) != 0)
    {  
        printf("sendd msg fail!\n");
    }

    free(buff);
}

void
NetWork::listener_cb(struct evconnlistener *listener, evutil_socket_t fd,
        struct sockaddr *sa, int socklen, void *user_data)
{
    NetWork* pNet = (NetWork*)user_data;
    struct event_base *base = pNet->m_base;
    struct bufferevent *bev;

    bev = bufferevent_socket_new(base, fd, BEV_OPT_CLOSE_ON_FREE);
    if (!bev) {
        fprintf(stderr, "Error constructing bufferevent!");
        event_base_loopbreak(base);
        return;
    }

    bufferevent_setcb(bev, conn_readcb, conn_writecb, conn_eventcb, pNet);
    bufferevent_enable(bev, EV_READ|EV_WRITE);


    //bufferevent_enable(bev, EV_WRITE);
    //bufferevent_disable(bev, EV_READ);
    
    /*
    int msgLen = strlen(MESSAGE);
    fprintf(stderr, "msgLen:%d\n", msgLen);
    for (int i = 0, iMax = 10; i < iMax; ++i)
    {
        bufferevent_write(bev, MESSAGE, msgLen);
    }
    */
}

void
NetWork::conn_readcb(struct bufferevent *bev, void *ctx)
{
    struct evbuffer *input = bufferevent_get_input(bev);
    size_t len = evbuffer_get_length(input);
    fprintf(stderr, "rec len = %ld", len);

    char data[128];
    int rbLen = evbuffer_remove(input, data, 128);
    fprintf(stderr, "rbLen:%d\n", rbLen);
    
    Person person;
    person.ParseFromArray(data, rbLen);
    fprintf(stderr, "Id:%d\n", person.id());
    fprintf(stderr, "Name:%s\n", person.name().c_str());
    fprintf(stderr, "Address.line1:%s\n", person.address().line1().c_str());
    fprintf(stderr, "Address.line2:%s\n", person.address().line2().c_str());
}

void
NetWork::conn_writecb(struct bufferevent *bev, void *user_data)
{
   // struct evbuffer *output = bufferevent_get_output(bev);
//    if (evbuffer_get_length(output) == 0) {
        printf("flushed answer\n");
 //       bufferevent_free(bev);
  //  }
}

void
NetWork::conn_eventcb(struct bufferevent *bev, short events, void *user_data)
{
    NetWork* pNet = (NetWork*)user_data;
    if (events & BEV_EVENT_EOF) 
    {
        printf("Connection closed.\n");
    } 
    else if (events & BEV_EVENT_ERROR) 
    {
        printf("Got an error on the connection: %s\n", strerror(errno));
    }
    else if (events & BEV_EVENT_CONNECTED)//主动连接回调 
    {
        if (!pNet->IsBind(bev))
        {
            std::shared_ptr<Session> session = Session::MakeSession();
            pNet->BindSession(session, bev);
        }
        printf("connected\n");
    }

    /* None of the other events can happen here, since we haven't enabled
     *   * timeouts */
    bufferevent_free(bev);
}

bool NetWork::IsBind(std::shared_ptr<Session> session)
{
    auto itr = this->m_mapSession.find(session);
    return itr == this->m_mapSession.end();
}

bool NetWork::IsBind(struct bufferevent* bev)
{
    auto itr = this->m_mapBufEvt.find(bev);
    return itr == this->m_mapBufEvt.end();
}

void
NetWork::BindSession(std::shared_ptr<Session> session, struct bufferevent * bev)
{
    this->m_mapSession.insert(std::pair<std::shared_ptr<Session>, struct bufferevent*>(session, bev));
    this->m_mapBufEvt.insert(std::pair<struct bufferevent*, std::shared_ptr<Session>>(bev, session));
}

void
NetWork::UnBindSession(std::shared_ptr<Session> session)
{
    struct bufferevent* bufEvt = NULL;

    auto itr = this->m_mapSession.find(session);
    if (itr != this->m_mapSession.end())
    {
        this->m_mapSession.erase(itr);
        bufEvt = itr->second;
    }

    if (bufEvt == NULL)
    {
        fprintf(stderr, "can not find bufEvt\n");
        return;
    }

    auto itr2 = this->m_mapBufEvt.find(bufEvt);
    if (itr2 != this->m_mapBufEvt.end())
    {
        this->m_mapBufEvt.erase(itr2);
    }

    if (this->m_mapSession.size() != this->m_mapBufEvt.size())
    {
        fprintf(stderr, "UnBindSession error mapSession.size=%d mapBufEvt.size=%d\n", (int)this->m_mapSession.size(), (int)this->m_mapBufEvt.size());
    }
}
