#include <stdio.h>

#include "common/Timer.h" 
#include "network/NetWork.h"
#include "pb/Hellow.pb.h"

using namespace std::placeholders;

NetWork net; 
std::shared_ptr<Session> session;
void RandPacket()
{
    printf("sdfsdf\n");
    Hellow h;
    h.set_msg("hi");
    net.Send(session, 2, &h);
}

void DealNetRecv(std::shared_ptr<Session> session, std::shared_ptr<NetPacket> pkg)
{
    printf("recv data\n");
}

int
main()
{
    Timer t;
    t.create(0, 1000, RandPacket);

    net.Start();
    NetReceiveDef cbRecv = std::bind(DealNetRecv, _1, _2);
    session = net.Connect("127.0.0.1", 56789, NULL, cbRecv);

    while(true)
    {
        net.Loop();    
    }

    return 0;
}
