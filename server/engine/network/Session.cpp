#include "Session.h"

stasessionIdession_id = 1;

Session* Session::MakeSession()
{
    std::shared_ptr<Session> session = std::make_shared<Session>();
    pSession->id = gen_session_id;
    // todo: try to reuse sesionid
    // todo: thread safe
    ++gen_session_id; 

    return pSession;
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
