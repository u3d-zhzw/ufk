#include <stdio.h>
#include <string.h>
#include <errno.h> 
#include<unistd.h>

#include <event2/bufferevent.h>
#include <event2/buffer.h>
#include <event2/listener.h>
#include <event2/util.h>
#include <event2/event.h>

#include "pb/Person.pb.h"

static const char MESSAGE[] = "Hello, World!\n";

static const int PORT = 56789;
static void listener_cb(struct evconnlistener *, evutil_socket_t,
            struct sockaddr *, int socklen, void *);
static void conn_readcb(struct bufferevent *bev, void *ctx);
static void conn_writecb(struct bufferevent *, void *);
static void conn_eventcb(struct bufferevent *, short, void *);
static void signal_cb(evutil_socket_t, short, void *);

static void
listener_cb(struct evconnlistener *listener, evutil_socket_t fd,
        struct sockaddr *sa, int socklen, void *user_data)
{
    struct event_base *base = (event_base*)user_data;
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
    bufferevent_write(bev, MESSAGE, msgLen);
}

static void
conn_readcb(struct bufferevent *bev, void *ctx)
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

static void
conn_writecb(struct bufferevent *bev, void *user_data)
{
   // struct evbuffer *output = bufferevent_get_output(bev);
//    if (evbuffer_get_length(output) == 0) {
        printf("flushed answer\n");
 //       bufferevent_free(bev);
  //  }
}

static void
conn_eventcb(struct bufferevent *bev, short events, void *user_data)
{
    if (events & BEV_EVENT_EOF) {
        printf("Connection closed.\n");
    } else if (events & BEV_EVENT_ERROR) {
        printf("Got an error on the connection: %s\n",
                strerror(errno));/*XXX win32*/
    }
    /* None of the other events can happen here, since we haven't enabled
     *   * timeouts */
    bufferevent_free(bev);
}

int
main(int argc, char** argv)
{
    printf("---\n");
    fprintf(stderr, "pid:%d\n", (int)getpid());
    printf("---\n");

    struct event_base *base;
    struct evconnlistener *listener;
    struct sockaddr_in sin;

    base = event_base_new();
    if (!base) {
        fprintf(stderr, "Could not initialize libevent!\n");
        return 1;
    }

    memset(&sin, 0, sizeof(sin));
    sin.sin_family = AF_INET;
    sin.sin_port = htons(PORT);

    listener = evconnlistener_new_bind(base, listener_cb, (void *)base,
            LEV_OPT_REUSEABLE|LEV_OPT_CLOSE_ON_FREE, -1,
            (struct sockaddr*)&sin,
            sizeof(sin));

    if (!listener) {
        fprintf(stderr, "Could not create a listener!\n");
        return 1;
    }

    event_base_dispatch(base);

    evconnlistener_free(listener);
    event_base_free(base);

    return 0;
}




