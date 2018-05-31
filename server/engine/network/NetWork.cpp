#include <algorithm>
#include <netinet/in.h>

#include "NetWork.h"
#include "pb/Person.pb.h"

unsigned short HEAD_SIZE = 7;

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
    if (this->m_listener != NULL)
    {
        evconnlistener_free(this->m_listener);
    }
    this->m_listener = NULL;

    if (this->m_base != NULL)
    {
        event_base_free(this->m_base);
    }
    this->m_base = NULL;

    for (auto i = this->m_mapSession.begin(); i != this->m_mapSession.end(); ++i)
    {
        this->UnBindSession(i->first);
    }
    this->m_mapSession.clear();

    for (auto i = this->m_mapBufEvt.begin(); i != this->m_mapBufEvt.end(); ++i)
    {
        this->UnBind(i->first);
    }
    this->m_mapBufEvt.clear();
}

void NetWork::Loop()
{
    if (this->m_base != NULL)
    {
        event_base_loop(this->m_base, EVLOOP_NONBLOCK);
    }
}

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

    printf("connect bev point address %p\n", bev);

    this->m_cbNetStatus = cbStatus;
    this->m_cbRecv = cbRecv;

    std::shared_ptr<Session> session = std::make_shared<Session>();
    this->BindSession(session, bev);

    bufferevent_setcb(bev, conn_readcb, NULL, conn_eventcb, this);
    bufferevent_enable(bev, EV_READ|EV_WRITE);
    bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0);

    return session;
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

void NetWork::Send(std::shared_ptr<Session> session, unsigned short id, ::google::protobuf::MessageLite* msg)
{
    auto itr = this->m_mapSession.find(session);
    if (itr == this->m_mapSession.end())
    {
        return;
    }

    // todo: add crc
    size_t bodySize = msg->ByteSizeLong();
    unsigned short totalSize = HEAD_SIZE + bodySize;

    char* buff = (char*)malloc(totalSize);
    *((unsigned char*)buff) = 1;                // packet type
    *((unsigned short*)(buff + 1)) = htons(id);          // identify id
    *((unsigned short*)(buff + 3)) = htons(bodySize);  // body size
    *((unsigned short*)(buff + 5)) = htons(totalSize); // packet size

    msg->SerializeToArray(buff + HEAD_SIZE, bodySize);

    this->Send(session, buff, totalSize);
    printf("bodysize:%d\n", (int)bodySize);

    free(buff);
}

void NetWork::Send(std::shared_ptr<Session> session, ProtcolId id, void* body, size_t bodySize)
{
    if (body == NULL)
    {
        return ;
    }

    unsigned short totalSize = HEAD_SIZE + bodySize;

    char* buff = (char*)malloc(totalSize);
    *((unsigned char*)buff) = 1;                        // packet type
    *((unsigned short*)(buff + 1)) = htons(id);         // identify id
    *((unsigned short*)(buff + 3)) = htons(bodySize);  // body size
    *((unsigned short*)(buff + 5)) = htons(totalSize); // packet size

    memcpy(buff + HEAD_SIZE, body , bodySize);

    this->Send(session, buff, totalSize);
    printf("bodysize:%d\n", (int)bodySize);

    free(buff);
}

void NetWork::Send(std::shared_ptr<Session> session, void* data, size_t size)
{
    bufferevent* bev = this->GetBufferevent(session);
    if (bev == NULL)
    {
        return ;
    }
 
    struct evbuffer* outbuffer = bufferevent_get_output(bev);
    //if(evbuffer_add_reference(outbuffer, buff, totalSize , NULL, NULL) != 0)
    if (bufferevent_write(bev, data, size) != 0)
    {   
        printf("sendd msg fail!\n");
    }
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

    bufferevent_setcb(bev, conn_readcb, NULL, conn_eventcb, pNet);
    bufferevent_enable(bev, EV_READ);
    bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0);

    std::shared_ptr<Session> session = Session::MakeSession();
    pNet->BindSession(session, bev);
    printf("listen\n");
}

void
NetWork::conn_readcb(struct bufferevent *bev, void *ctx)
{
    NetWork* pNet = (NetWork*) ctx;

    std::shared_ptr<Session> session = pNet->GetSession(bev);
    if (session == NULL)
    {
        printf("can not find session for bev:%p\n", bev);
        return;
    }

    struct evbuffer *input = bufferevent_get_input(bev);
    size_t recvLen = evbuffer_get_length(input);
    if (recvLen < HEAD_SIZE)
    {
        bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0); 
        return;
    }
    
    unsigned short pkgSize = 0;
    struct evbuffer_ptr ptr;
    evbuffer_ptr_set(input, &ptr, 5, EVBUFFER_PTR_SET);
    evbuffer_copyout_from(input, &ptr, &pkgSize, 2);
    pkgSize = ntohs(pkgSize);
    if (recvLen < pkgSize)
    {
        bufferevent_setwatermark(bev, EV_READ, pkgSize, 0); 
        return ;
    }

    char* data= (char*) malloc(pkgSize);
    int buffSize = evbuffer_remove(input, data, pkgSize);

    unsigned char type = *((unsigned char*)data);
    unsigned short id = ntohs(*((unsigned short*)(data + 1)));
    unsigned short bodySize = ntohs(*((unsigned short*)(data + 3)));
    //        unsigned short pkgSize = ntohs(*((unsigned short*)(data + 5)));

    // todo: 
    // 1. deal with packet type
    // 2. check crc
    if (pNet->m_cbRecv != NULL)
    {
        pNet->m_cbRecv(session, id, data + HEAD_SIZE, bodySize);
    }

    // deal with next packet
    recvLen = evbuffer_get_length(input);
    if (recvLen > 0 )
    {
        conn_readcb(bev, ctx);
    }

    free(data);
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
        pNet->UnBind(bev);
    } 
    else if (events & BEV_EVENT_ERROR) 
    {
        printf("Got an error on the connection: %s\n", strerror(errno));
    }
    else if (events & BEV_EVENT_CONNECTED)//主动连接回调 
    {
        printf("event bev point address %p\n", bev);
        if (!pNet->IsBind(bev))
        {
            printf("bind\n");
            std::shared_ptr<Session> session = Session::MakeSession();
            pNet->BindSession(session, bev);
        }
        printf("connected\n");
    }
}

bool NetWork::IsBind(std::shared_ptr<Session> session)
{
    auto itr = this->m_mapSession.find(session);
    return itr != this->m_mapSession.end();
}

bool NetWork::IsBind(struct bufferevent* bev)
{
    auto itr = this->m_mapBufEvt.find(bev);
    return itr != this->m_mapBufEvt.end();
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
    struct bufferevent* bev = NULL;

    auto itr = this->m_mapSession.find(session);
    if (itr != this->m_mapSession.end())
    {
        this->m_mapSession.erase(itr);
        bev = itr->second;
    }

    if (bev == NULL)
    {
        fprintf(stderr, "can not find bufEvt\n");
        return;
    }

    auto itr2 = this->m_mapBufEvt.find(bev);
    if (itr2 != this->m_mapBufEvt.end())
    {
        this->m_mapBufEvt.erase(itr2);
    }

    if (this->m_mapSession.size() != this->m_mapBufEvt.size())
    {
        fprintf(stderr, "UnBindSession error mapSession.size=%d mapBufEvt.size=%d\n", (int)this->m_mapSession.size(), (int)this->m_mapBufEvt.size());
    }

    bufferevent_free(bev);
}

void NetWork::UnBind(struct bufferevent* bev)
{
    std::shared_ptr<Session> session = NULL;

    auto itrBev = this->m_mapBufEvt.find(bev);
    if (itrBev != this->m_mapBufEvt.end())
    {
        this->m_mapBufEvt.erase(itrBev);
        session = itrBev->second;
    }

    if (session == NULL)
    {
        fprintf(stderr, "can not find session\n");
        return;
    }

    auto itrSession = this->m_mapSession.find(session);
    if (itrSession != this->m_mapSession.end())
    {
        this->m_mapSession.erase(itrSession);
    }

    if (this->m_mapSession.size() != this->m_mapBufEvt.size())
    {
        fprintf(stderr, "UnBindSession error mapSession.size=%d mapBufEvt.size=%d\n", (int)this->m_mapSession.size(), (int)this->m_mapBufEvt.size());
    }

    bufferevent_free(bev);
}

std::shared_ptr<Session> NetWork::GetSession(struct bufferevent* bev)
{
    auto itr = this->m_mapBufEvt.find(bev);
    if (itr == this->m_mapBufEvt.end())
    {
        return NULL;
    }

    return itr->second;
}

struct bufferevent* NetWork::GetBufferevent(std::shared_ptr<Session> session)
{
    auto itr = this->m_mapSession.find(session);
    if (itr == this->m_mapSession.end())
    {
        return NULL;
    }

    return itr->second;
}

