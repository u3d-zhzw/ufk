#include <algorithm>
#include <netinet/in.h>

#include "TcpConnection.h"
#include "pb/Person.pb.h"

unsigned short HEAD_SIZE = 7;
unsigned short PKG_MAX_SIZE = 1024;

bool TcpConnection::Start()
{
    this->evt_base_ = event_base_new();
    if (!this->evt_base_) {
        fprintf(stderr, "Could not initialize libevent!\n");
        return false;
    }

    return true;
}

void TcpConnection::Stop()
{
    if (this->evt_listener_ != NULL)
    {
        evconnlistener_free(this->evt_listener_);
    }
    this->evt_listener_ = NULL;

    if (this->evt_base_ != NULL)
    {
        event_base_free(this->evt_base_);
    }
    this->evt_base_ = NULL;

    for (auto i = this->map_session_.begin(); i != this->map_session_.end(); ++i)
    {
        this->UnBindSession(i->first);
    }
    this->map_session_.clear();

    for (auto i = this->map_bufevt_.begin(); i != this->map_bufevt_.end(); ++i)
    {
        this->UnBind(i->first);
    }
    this->map_bufevt_.clear();
}

void TcpConnection::Loop()
{
    if (this->evt_base_ != NULL)
    {
        event_base_loop(this->evt_base_, EVLOOP_NONBLOCK);
    }
}

std::shared_ptr<Session> TcpConnection::Connect(string ip, int port, NetStatueDef cb_status, NetReceiveDef cb_recv)
{
    struct bufferevent* bev = bufferevent_socket_new(this->evt_base_, -1, BEV_OPT_CLOSE_ON_FREE);
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

    this->cb_net_status_ = cb_status;
    this->cb_recv_ = cb_recv;

    std::shared_ptr<Session> session = std::make_shared<Session>();
    this->BindSession(session, bev);

    bufferevent_setcb(bev, conn_readcb, NULL, conn_eventcb, this);
    bufferevent_enable(bev, EV_READ|EV_WRITE);
    bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0);

    return session;
}

void TcpConnection::Listen(int port, NetStatueDef cb_status, NetReceiveDef cb_recv)
{
    this->cb_net_status_ = cb_status;
    this->cb_recv_ = cb_recv;

    struct sockaddr_in sin;
    memset(&sin, 0, sizeof(sin));
    sin.sin_family = AF_INET;
    sin.sin_port = htons(port);

    this->evt_listener_ = evconnlistener_new_bind(this->evt_base_, listener_cb, (void *)this,
            LEV_OPT_REUSEABLE|LEV_OPT_CLOSE_ON_FREE, -1,
            (struct sockaddr*)&sin,
            sizeof(sin));

    if (!this->evt_listener_)
    {
        fprintf(stderr, "Could not create a listener!\n");
        return ;
    }
}

void TcpConnection::Send(std::shared_ptr<Session> session, unsigned short id, ::google::protobuf::MessageLite* msg)
{
    auto itr = this->map_session_.find(session);
    if (itr == this->map_session_.end())
    {
        return;
    }

    // todo: add crc
    size_t body_size = msg->ByteSizeLong();
    unsigned short total_size = HEAD_SIZE + body_size;

    char* buf = (char*)malloc(total_size);
    *((unsigned char*)buf) = 1;                // packet type
    *((unsigned short*)(buf + 1)) = htons(id);          // identify id
    *((unsigned short*)(buf + 3)) = htons(body_size);  // body size
    *((unsigned short*)(buf + 5)) = htons(total_size); // packet size

    msg->SerializeToArray(buf + HEAD_SIZE, body_size);

    this->Send(session, buf, total_size);
    printf("bodysize:%d\n", (int)body_size);

    free(buf);
}

void TcpConnection::Send(std::shared_ptr<Session> session, ProtcolId id, void* body, size_t body_size)
{
    if (body == NULL)
    {
        return ;
    }

    unsigned short total_size = HEAD_SIZE + body_size;

    char* buf = (char*)malloc(total_size);
    *((unsigned char*)buf) = 1;                        // packet type
    *((unsigned short*)(buf + 1)) = htons(id);         // identify id
    *((unsigned short*)(buf + 3)) = htons(body_size);  // body size
    *((unsigned short*)(buf + 5)) = htons(total_size); // packet size

    memcpy(buf + HEAD_SIZE, body , body_size);

    this->Send(session, buf, total_size);
    printf("bodysize:%d\n", (int)body_size);

    free(buf);
}

void TcpConnection::Send(std::shared_ptr<Session> session, void* data, size_t size)
{
    bufferevent* bev = this->GetBufferevent(session);
    if (bev == NULL)
    {
        return ;
    }
 
    struct evbuffer* out_buffer = bufferevent_get_output(bev);
    //if(evbuffer_add_reference(outbuffer, buff, totalSize , NULL, NULL) != 0)
    if (bufferevent_write(bev, data, size) != 0)
    {   
        printf("sendd msg fail!\n");
    }
}

void
TcpConnection::listener_cb(struct evconnlistener *listener, evutil_socket_t fd,
        struct sockaddr *sa, int socklen, void *user_data)
{
    TcpConnection *conn = (TcpConnection*)user_data;
    struct event_base *base = conn->evt_base_;
    struct bufferevent *bev;

    bev = bufferevent_socket_new(base, fd, BEV_OPT_CLOSE_ON_FREE);
    if (!bev) {
        fprintf(stderr, "Error constructing bufferevent!");
        event_base_loopbreak(base);
        return;
    }

    bufferevent_setcb(bev, conn_readcb, NULL, conn_eventcb, conn);
    bufferevent_enable(bev, EV_READ);
    bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0);

    std::shared_ptr<Session> session = std::make_shared<Session>();
    conn->BindSession(session, bev);
    printf("listen\n");
}

void
TcpConnection::conn_readcb(struct bufferevent *bev, void *ctx)
{
    TcpConnection *conn = (TcpConnection*) ctx;

    std::shared_ptr<Session> session = conn->GetSession(bev);
    if (session == NULL)
    {
        printf("can not find session for bev:%p\n", bev);
        return;
    }

    struct evbuffer *input = bufferevent_get_input(bev);
    size_t recv_len = evbuffer_get_length(input);
    if (recv_len < HEAD_SIZE)
    {
        bufferevent_setwatermark(bev, EV_READ, HEAD_SIZE, 0); 
        return;
    }

    struct evbuffer_ptr ptr;
    evbuffer_ptr_set(input, &ptr, 5, EVBUFFER_PTR_SET);

    unsigned short pkg_size = 0;
    evbuffer_copyout_from(input, &ptr, &pkg_size, 2);
    pkg_size = ntohs(pkg_size);

    if (recv_len < pkg_size)
    {
        bufferevent_setwatermark(bev, EV_READ, pkg_size, 0); 
        return ;
    }

    if (pkg_size > PKG_MAX_SIZE)
    {
        //TODO: disconnect
        printf("pkgSize over %d\n", PKG_MAX_SIZE);
        return;
    } 

    char* buf= (char*) malloc(pkg_size);
    int buf_size = evbuffer_remove(input, buf, pkg_size);

    if (buf_size != pkg_size)
    {
        //TODO: disconnect
        printf("event buffer error");
    }

    unsigned char type = *((unsigned char*)buf);
    unsigned short id = ntohs(*((unsigned short*)(buf + 1)));
    unsigned short body_size = ntohs(*((unsigned short*)(buf + 3)));

    // todo: 
    // 1. deal with packet type
    // 2. check crc
    if (conn->cb_recv_ != NULL)
    {
        conn->cb_recv_(session, id, buf + HEAD_SIZE, body_size);
    }

    free(buf);

    // deal with next packet
    recv_len = evbuffer_get_length(input);
    if (recv_len > 0 )
    {
        conn_readcb(bev, ctx);
    }
}

void TcpConnection::conn_writecb(struct bufferevent *bev, void *ctx)
{
   // struct evbuffer *output = bufferevent_get_output(bev);
//    if (evbuffer_get_length(output) == 0) {
        printf("flushed answer\n");
 //       bufferevent_free(bev);
  //  }
}

void TcpConnection::conn_eventcb(struct bufferevent *bev, short events, void *ctx)
{
    TcpConnection* conn = (TcpConnection*)ctx;
    
    if (events & BEV_EVENT_EOF) 
    {
        printf("Connection closed.\n");
        conn->UnBind(bev);
    } 
    else if (events & BEV_EVENT_ERROR) 
    {
        printf("Got an error on the connection: %s\n", strerror(errno));
    }
    else if (events & BEV_EVENT_CONNECTED)//主动连接回调 
    {
        printf("event bev point address %p\n", bev);
        if (!conn->IsBind(bev))
        {
            printf("bind\n");
            std::shared_ptr<Session> session = std::make_shared<Session>();
            conn->BindSession(session, bev);
        }
        printf("connected\n");
    }
}

bool TcpConnection::IsBind(std::shared_ptr<Session> session)
{
    auto itr = this->map_session_.find(session);
    return itr != this->map_session_.end();
}

bool TcpConnection::IsBind(struct bufferevent* bev)
{
    auto itr = this->map_bufevt_.find(bev);
    return itr != this->map_bufevt_.end();
}

void
TcpConnection::BindSession(std::shared_ptr<Session> session, struct bufferevent * bev)
{
    this->map_session_.insert(std::pair<std::shared_ptr<Session>, struct bufferevent*>(session, bev));
    this->map_bufevt_.insert(std::pair<struct bufferevent*, std::shared_ptr<Session>>(bev, session));
}

void
TcpConnection::UnBindSession(std::shared_ptr<Session> session)
{
    struct bufferevent* bev = NULL;

    auto itr = this->map_session_.find(session);
    if (itr != this->map_session_.end())
    {
        this->map_session_.erase(itr);
        bev = itr->second;
    }

    if (bev == NULL)
    {
        fprintf(stderr, "can not find bufEvt\n");
        return;
    }

    auto itr2 = this->map_bufevt_.find(bev);
    if (itr2 != this->map_bufevt_.end())
    {
        this->map_bufevt_.erase(itr2);
    }

    if (this->map_session_.size() != this->map_bufevt_.size())
    {
        fprintf(stderr, "UnBindSession error mapSession.size=%d mapBufEvt.size=%d\n", (int)this->map_session_.size(), (int)this->map_bufevt_.size());
    }

    bufferevent_free(bev);
}

void TcpConnection::UnBind(struct bufferevent *bev)
{
    std::shared_ptr<Session> session = NULL;

    auto itrBev = this->map_bufevt_.find(bev);
    if (itrBev != this->map_bufevt_.end())
    {
        this->map_bufevt_.erase(itrBev);
        session = itrBev->second;
    }

    if (session == NULL)
    {
        fprintf(stderr, "can not find session\n");
        return;
    }

    auto itrSession = this->map_session_.find(session);
    if (itrSession != this->map_session_.end())
    {
        this->map_session_.erase(itrSession);
    }

    if (this->map_session_.size() != this->map_bufevt_.size())
    {
        fprintf(stderr, "UnBindSession error mapSession.size=%d mapBufEvt.size=%d\n", (int)this->map_session_.size(), (int)this->map_bufevt_.size());
    }

    bufferevent_free(bev);
}

std::shared_ptr<Session> TcpConnection::GetSession(struct bufferevent *bev)
{
    auto itr = this->map_bufevt_.find(bev);
    if (itr == this->map_bufevt_.end())
    {
        return NULL;
    }

    return itr->second;
}

struct bufferevent* TcpConnection::GetBufferevent(std::shared_ptr<Session> session)
{
    auto itr = this->map_session_.find(session);
    if (itr == this->map_session_.end())
    {
        return NULL;
    }

    return itr->second;
}

