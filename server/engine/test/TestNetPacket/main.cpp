#include <stdio.h>
#include <stdlib.h>
#include <time.h>  

#include "common/Timer.h" 
#include "network/NetWork.h"
#include "pb/Hellow.pb.h"
#include "pb/Person.pb.h"


using namespace std::placeholders;

NetWork net; 
std::shared_ptr<Session> session;

std::string gen_random(const int len) 
{
    static const char alphanum[] =
        "0123456789"
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        "abcdefghijklmnopqrstuvwxyz";

    std::string retStr;
    retStr.reserve(len);
    for (int i = 0; i < len; ++i)
    {
        retStr.push_back(alphanum[rand() % (sizeof(alphanum) - 1)]);
    }

    return retStr;
}

void RandPacket()
{
    time_t t;
    time(&t);

    ::google::protobuf::MessageLite* msg = NULL;
    unsigned short id = 1;
    time_t mod = 1;
    int len_mod = 15;
    if (t % mod == 0)
    {
        Person person;
        person.set_id(12345);
        person.set_name(gen_random(rand() % len_mod));

        Address adr;
        adr.set_line1(gen_random(rand() % len_mod));
        adr.set_line2(gen_random(rand() % len_mod));

        *(person.mutable_address()) = adr;
        net.Send(session, id, &person);
    }
    else
    {
        Hellow h;
        h.set_msg(gen_random(rand() % len_mod));
        msg = &h;
        net.Send(session, id, &h);
    }
}

void DealNetRecv(std::shared_ptr<Session> session, ProtcolId id, const void* data, unsigned short size)
{
    printf("recv data\n");
    printf("id:%d\n", id);
    printf("bodysize:%d\n", size);
}

int
main()
{
    srand (time(NULL));

    Timer t;
    t.create(0, 1000, RandPacket);

    net.Start();
    NetReceiveDef cbRecv = std::bind(DealNetRecv, _1, _2, _3, _4);
    session = net.Connect("127.0.0.1", 56789, NULL, cbRecv);

    while(true)
    {
        net.Loop();    
    }

    return 0;
}
