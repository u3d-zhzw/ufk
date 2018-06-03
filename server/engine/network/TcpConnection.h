#ifndef __TCPCONNECTION_H
#define __TCPCONNECTION_H

#include <string>
#include <unordered_map>
#include <memory>

#include <event2/bufferevent.h>
#include <event2/buffer.h>
#include <event2/listener.h>
#include <event2/util.h>
#include <event2/event.h>
#include <google/protobuf/stubs/common.h>
#include <google/protobuf/message_lite.h>

#include "common/Defines.h"
#include "network/Session.h"

using namespace std;

class TcpConnection
{
public:
    std::shared_ptr<Session> Connect(string ip, int port, NetStatueDef cbStatus, NetReceiveDef cbRecv);
    void Listen(int port, NetStatueDef status_cb, NetReceiveDef recv_cb);

    void Send(std::shared_ptr<Session> session, unsigned short id, ::google::protobuf::MessageLite *msg);
    void Send(std::shared_ptr<Session> session, ProtcolId id, void *body, size_t bodySize);
    void Send(std::shared_ptr<Session> session, void *data, size_t size);

public:
    bool Start();
    void Stop();
    void Loop();

private:
    static void listener_cb(struct evconnlistener *listener, evutil_socket_t fd,
        struct sockaddr *sa, int socklen, void *user_data);
    static void conn_readcb(struct bufferevent *bev, void *ctx);
    static void conn_writecb(struct bufferevent *bev, void *ctx);
    static void conn_eventcb(struct bufferevent *bev, short events, void *ctx);
    static void signal_cb(evutil_socket_t, short events, void *ctx);

private:
    void BindSession(std::shared_ptr<Session> session, struct bufferevent *bev);
    void UnBindSession(std::shared_ptr<Session> session);
    void UnBind(struct bufferevent *bev);
    std::shared_ptr<Session> GetSession(struct bufferevent *bev);
    struct bufferevent* GetBufferevent(std::shared_ptr<Session> session);
    bool IsBind(std::shared_ptr<Session> session);
    bool IsBind(struct bufferevent *bev);

private:
    struct event_base* evt_base_;
    struct evconnlistener* evt_listener_;

    std::unordered_map<std::shared_ptr<Session>, struct bufferevent *> map_session_;
    std::unordered_map<struct bufferevent *, std::shared_ptr<Session>> map_bufevt_;

    NetStatueDef cb_net_status_;
    NetReceiveDef cb_recv_;
};

#endif //__TCPCONNECTION_H
