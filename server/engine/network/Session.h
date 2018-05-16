#ifndef __SESSION_H
#define __SESSION_H

#include "Defines.h"

class Session
{
private:

public:
    SessionId id;

public:
    static Session* MakeSession();
    static void FreeSession(Session* pSession);
    
}
#endif //__SESSION_H
