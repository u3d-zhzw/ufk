#include <stdio.h>

#include "common/Timer.h" 
#include "network/NetWork.h"

NetWork net; 
void RandPacket()
{
    printf("sdfsdf\n");
}

int
main()
{
    Timer t;
    t.create(0, 1000, RandPacket);

    net.Connect("127.0.0.1", 56789, NULL, NULL);

    getchar();
    return 0;
}
