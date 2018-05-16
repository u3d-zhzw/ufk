#ifndef __APPLICATION_H
#define __APPLICATION_H

#include "network/NetWork.h"

class Application
{
public:
       virtual bool Start(); 
       virtual void Stop();
       virtual void Loop();

public:
       void Send(ProtcolId main, ProtcolId sub, ::google::protobuf::MessageLite& msg);

private:
    NetWork* m_net;
};
#endif //__APPLICATION_H
