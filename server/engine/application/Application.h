#ifndef __APPLICATION_H
#define __APPLICATION_H

#include <memory>

#include "common/Defines.h"
#include "network/TcpConnection.h"
#include "network/Session.h"

class Application
{
public:
       virtual bool Start();
       virtual void Stop();
       virtual void Loop();

public:
       void Send(std::shared_ptr<Session> session, short id, ::google::protobuf::MessageLite *msg);

private:
       void ConnReceive(std::shared_ptr<Session> session, ProtcolId id, const void *data, unsigned short size);

private:
    TcpConnection* tcp_conn_;
};
#endif //__APPLICATION_H
