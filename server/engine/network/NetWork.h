#ifndef __NETWORK_H
#define __NETWORK_H

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

class NetWork
{
public:
    std::shared_ptr<Session> Connect(string ip, int port, NetStatueDef cbStatus, NetReceiveDef cbRecv);
    void Listen(int port, NetStatueDef status_cb, NetReceiveDef recv_cb);

    void Send(std::shared_ptr<Session> session, unsigned short id, ::google::protobuf::MessageLite* msg);
    void Send(std::shared_ptr<Session> session, void* data, size_t size);

public:
    bool Start();
    void Stop();
    void Loop();

private:
    static void listener_cb(struct evconnlistener *, evutil_socket_t,
            struct sockaddr *, int socklen, void *);
    static void conn_readcb(struct bufferevent *bev, void *ctx);
    static void conn_writecb(struct bufferevent *, void *);
    static void conn_eventcb(struct bufferevent *, short, void *);
    static void signal_cb(evutil_socket_t, short, void *);

private:
    void BindSession(std::shared_ptr<Session> session, struct bufferevent * bev);
    void UnBindSession(std::shared_ptr<Session> session);
    std::shared_ptr<Session> GetSession(struct bufferevent* bev);
    struct bufferevent* GetEventBuffer(std::shared_ptr<Session> session);
    bool IsBind(std::shared_ptr<Session> session);
    bool IsBind(struct bufferevent* bev);

private:
    struct event_base* m_base;
    struct evconnlistener* m_listener;

    std::unordered_map<std::shared_ptr<Session>, struct bufferevent *> m_mapSession;
    std::unordered_map<struct bufferevent *, std::shared_ptr<Session>> m_mapBufEvt;
    std::unordered_map<struct bufferevent *, std::string> m_mapBevStream;

    NetStatueDef m_cbNetStatus;
    NetReceiveDef m_cbRecv;
};

#endif //__NETWORK_H
