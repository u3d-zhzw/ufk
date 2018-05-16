#ifndef __NETWORK_H
#define __NETWORK_H

#include <string>
#include <unordered_map>

#include <event2/bufferevent.h>
#include <event2/buffer.h>
#include <event2/listener.h>
#include <event2/util.h>
#include <event2/event.h>





using namespace std;

class NetWork
{
public:
    void Connect(string ip, int port);
    void Listen(int port);
    void Send(SessionId id, void* data, size_t size);
    void Send(Session* pSession, void* data size_t size);

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

private:
    struct event_base* m_base;
    struct evconnlistener* m_listener;

    std::unordered_map<std::shared_ptr<Session>, struct bufferevent *> m_mapSeesion;
};

#endif //__NETWORK_H
