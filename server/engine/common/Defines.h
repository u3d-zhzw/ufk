#ifndef __DEFINES_H
#define __DEFINES_H

#include <functional>
#include <memory>

class Session;
class NetPacket;


typedef unsigned int SessionId;
typedef unsigned short ProtcolId;

/**
 * @breif 网络状态
 */
enum E_TCPCONNSATUS
{
    CONNECTED,
    CLOSE,
    MAX,
};

typedef std::function<void(E_TCPCONNSATUS)> NetStatueDef;
typedef std::function<void(std::shared_ptr<Session>, ProtcolId id, const void* data, unsigned short size)> NetReceiveDef;

#endif //__DEFINES_H
