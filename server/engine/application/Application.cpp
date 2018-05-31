#include "application/Application.h"
#include "pb/Person.pb.h"
#include "pb/Hellow.pb.h"

using namespace std::placeholders;

bool Application::Start()
{
    this->m_net = new NetWork();

    NetReceiveDef cbConnReceive = std::bind(&Application::ConnReceive, this, _1, _2, _3, _4);
    this->m_net->Start();
    this->m_net->Listen(56789, NULL, cbConnReceive);

    this->Loop();

    return true;
}

void Application::Stop()
{
    if (this->m_net != NULL)
    {
        this->m_net->Stop();
        delete this->m_net;
    }
    this->m_net = NULL;
}

void Application::Loop()
{
    while(true)
    {
        if (this->m_net != NULL)
        {
            this->m_net->Loop();
        }
    }
}

void Application::Send(std::shared_ptr<Session> session, short id, ::google::protobuf::MessageLite* msg)
{
    this->m_net->Send(session, id, msg);
}

void Application::ConnReceive(std::shared_ptr<Session> session, ProtcolId id, const void* data, unsigned short size)
{
    printf("id:%d\n", id);
    printf("sessionId:%d\n", session->id);
    
    if (id == 1)
    {
        Person p;
        p.ParseFromArray(data, size);
        printf("Person.id:%d\n", p.id());
        printf("Person.name:%s\n", p.name().c_str());
        printf("Person.Address.line1:%s\n", p.address().line1().c_str());
        printf("Person.Address.line2:%s\n", p.address().line2().c_str());

        this->m_net->Send(session, 2, (void*)data, size);
    }
    else if (id == 3)
    {
        Hellow h;
        h.ParseFromArray(data, size);
        printf("Hellow.msg:%s\n", h.msg().c_str());
        this->m_net->Send(session, 4, (void*)data, size);
    }
}
