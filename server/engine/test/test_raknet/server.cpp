#include "raknet/RakPeerInterface.h"
#include "raknet/RakNetTypes.h"
#include "raknet/MessageIdentifiers.h"
#include "raknet/RakSleep.h"
#include "raknet/RakString.h"
#include "raknet/BitStream.h"

#include "stdio.h"
#include "defines.h"

int main()
{
    RakNet::RakPeerInterface* peer = RakNet::RakPeerInterface::GetInstance();
    RakNet::SocketDescriptor sd(server_port, 0);

    peer->Startup(max_connections_allowed, &sd, 1);
    peer->SetIncomingPassword(conn_pwd, strlen(conn_pwd));
    peer->SetMaximumIncomingConnections(max_players_per_server);//设置允许有多少连接

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
                    printf("ID_CONNECTION_REQUEST_ACCEPTED from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
                case ID_CONNECTION_LOST:
                    printf("ID_CONNECTION_LOST from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
                case ID_DISCONNECTION_NOTIFICATION:
                    printf("ID_DISCONNECTION_NOTIFICATION from %s,guid=%s\n", packet->systemAddress.ToString(true), packet->guid.ToString());
                    break;
                    // Login
                case ID_USER_PACKET_ENUM:
                    {
                        RakNet::BitStream bsIn(packet->data, packet->length, false);
                        bsIn.IgnoreBytes(sizeof(RakNet::MessageID));
                        RakNet::RakString msg;
                        bsIn.Read(msg);
                        printf("rec msg %s\n", msg.C_String());
                    }
                    break;
            }
        }
        RakSleep(30);
    }

    peer->Shutdown(300);
    RakNet::RakPeerInterface::DestroyInstance(peer);

    return 0;
}
