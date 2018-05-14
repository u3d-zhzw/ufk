#include "NetWork.h"

#include "pb/Person.pb.h"

static const char MESSAGE[] = "Hello, World!\n";


void NetWork::Connect(string ip, int port)
{
    // TODO:
}

void NetWork::Listen(int port)
{
    this->m_base = event_base_new();
    if (!this->m_base) {
        fprintf(stderr, "Could not initialize libevent!\n");
        return ;
    }

    struct sockaddr_in sin;
    memset(&sin, 0, sizeof(sin));
    sin.sin_family = AF_INET;
    sin.sin_port = htons(port);

    this->m_listener = evconnlistener_new_bind(this->m_base, listener_cb, (void *)this,
            LEV_OPT_REUSEABLE|LEV_OPT_CLOSE_ON_FREE, -1,
            (struct sockaddr*)&sin,
            sizeof(sin));

    if (!this->m_listener) {
        fprintf(stderr, "Could not create a listener!\n");
        return ;
    }
}

bool NetWork::Start()
{
    return true;
}

void NetWork::Stop()
{
    evconnlistener_free(this->m_listener);
    event_base_free(this->m_base);
}

void NetWork::Loop()
{
    event_base_loop(this->m_base, EVLOOP_NONBLOCK);
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

    bufferevent_setcb(bev, conn_readcb, conn_writecb, conn_eventcb, NULL);
    bufferevent_enable(bev, EV_READ);
    //bufferevent_enable(bev, EV_WRITE);
    //bufferevent_disable(bev, EV_READ);
    
    int msgLen = strlen(MESSAGE);
    fprintf(stderr, "msgLen:%d\n", msgLen);
    for (int i = 0, iMax = 10; i < iMax; ++i)
    {
        bufferevent_write(bev, MESSAGE, msgLen);
    }
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
    if (events & BEV_EVENT_EOF) {
        printf("Connection closed.\n");
    } else if (events & BEV_EVENT_ERROR) {
        printf("Got an error on the connection: %s\n",
                strerror(errno));/*XXX win32*/ }
    /* None of the other events can happen here, since we haven't enabled
     *   * timeouts */
    bufferevent_free(bev);
}
