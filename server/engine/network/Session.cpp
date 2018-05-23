#include "Session.h"

SessionId gen_session_id = 1;

std::shared_ptr<Session> Session::MakeSession()
{
    std::shared_ptr<Session> session = std::make_shared<Session>();
    session->id = gen_session_id;
    // todo: try to reuse sesionid
    // todo: thread safe
    ++gen_session_id;

    return session;
}


void Session::FreeSession(Session* pSession)
{
    // todo: try to reuse sesionid
    if (pSession != NULL)
    {
        delete pSession;
    }
    
    pSession = NULL;
}
