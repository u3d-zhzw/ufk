#include "application/Application.h"

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
    printf("data.len:%d\n", size);
}
