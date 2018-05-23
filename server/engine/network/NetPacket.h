#ifndef __NETPACKET_H
#define __NETPACKET_H

#include <google/protobuf/stubs/common.h>
#include <google/protobuf/message_lite.h>

class NetPacket
{

//public:
//    NetPacket(short id);

public:
    /*
    void SetHead();
    void GetHead();
    */

//    void SetBody(::google::protobuf::MessageLite msg);
//    const ::google::protobuf::MessageLite GetBody();

private:
    short m_Id;
    unsigned int m_size;
    void* m_body;
};
#endif //__NETPACKET_H
