#ifndef __APPLICATION_H
#define __APPLICATION_H

#include <memory>

#include "common/Defines.h"
#include "network/NetWork.h"
#include "network/NetPacket.h"
#include "network/Session.h"


class Application
{
public:
       virtual bool Start();
       virtual void Stop();
       virtual void Loop();

public:
       void Send(std::shared_ptr<Session> session, short id, ::google::protobuf::MessageLite* msg);

private:
       void ConnReceive(std::shared_ptr<Session> session, std::shared_ptr<NetPacket> pkg);

private:
    NetWork* m_net;
};
#endif //__APPLICATION_H
