
#include "application/Application.h"

bool Application::Start()
{
    this->m_net = new NetWork();
    this->m_net->Listen(56789);

    this->Loop();
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


void Application::Send(ProtcolId main, ProtcolId sub, ::google::protobuf::MessageLite& msg)
{
    size_t size = msg.ByteSizeLong();

    void* buff = malloc(size);
    msg.SerializeToArray(buff, size);

    this->m_net->Send();

    free(buff);
}
