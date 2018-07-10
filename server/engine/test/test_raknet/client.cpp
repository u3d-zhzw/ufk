#include "raknet/RakPeerInterface.h"
#include "raknet/RakNetTypes.h"
#include "raknet/MessageIdentifiers.h"
#include "raknet/RakSleep.h"
#include "raknet/BitStream.h"

#include "stdio.h"
#include "defines.h"

const char* message = "Hello World";

int main()
{
    RakNet::RakPeerInterface* peer = RakNet::RakPeerInterface::GetInstance();
    RakNet::SocketDescriptor sd;

    //1参数用于设置连接的最大值。2参数是一个线程休眠定时器。3参数描述了监听的端口/地址。
    peer->Startup(1, &sd, 1);

    //1参数用于设置服务器的IP地址或域地址。2参数是服务器端口。3、4 输入0。
    peer->Connect(server_ip, server_port, conn_pwd, strlen(conn_pwd));

    //1、字节流 2、有多少字节要发送 3、数据包的优先级 4、获取数据的序列和子串 5、使用哪个有序流 6、要发送到的远端系统（UNASSIGNED_RAKNET_GUID） 7、表明是否广播到所有的连接系统或不广播
     //peer->Send((char*)message, strlen(message)+1, HIGH_PRIORITY, RELIABLE, 0, RakNet::UNASSIGNED_SYSTEM_ADDRESS, true);

    RakNet::Packet *packet;
    while (1)
    {
        for (packet = peer->Receive(); packet; peer->DeallocatePacket(packet), packet = peer->Receive())
        {
            switch (packet->data[0])
            {
                case ID_NEW_INCOMING_CONNECTION:
                    printf("ID_NEW_INCOMING_CONNECTION from %s. guid=%s.\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
                case ID_CONNECTION_REQUEST_ACCEPTED:
                    {
                        printf("ID_CONNECTION_REQUEST_ACCEPTED from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                        printf("Logging in...\n");
                        RakNet::BitStream bsOut;
                        bsOut.WriteCasted<RakNet::MessageID>(ID_USER_PACKET_ENUM);
                        bsOut.Write(message);
                        peer->Send(&bsOut, HIGH_PRIORITY, RELIABLE_ORDERED, 0, packet->guid, false);
                    }
                    break;
                case ID_CONNECTION_LOST:
                    printf("ID_CONNECTION_LOST from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
                case ID_DISCONNECTION_NOTIFICATION:
                    printf("ID_DISCONNECTION_NOTIFICATION from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
            }
        }
        RakSleep(30);
    }

    peer->Shutdown(300);
    RakNet::RakPeerInterface::DestroyInstance(peer);

    return 0;
}
