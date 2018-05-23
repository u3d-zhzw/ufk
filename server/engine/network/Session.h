#ifndef __SESSION_H
#define __SESSION_H

#include "common/Defines.h"
#include <memory>

class Session
{
public:
    SessionId id;

public:
    static std::shared_ptr<Session> MakeSession();
    static void FreeSession(Session* pSession);
   
};
#endif //__SESSION_H
