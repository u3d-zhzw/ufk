#include <stdio.h>
#include <string.h>
#include <errno.h> 
#include<unistd.h>

#include "network/NetWork.h"

int
main(int argc, char** argv)
{
    printf("---\n");
    fprintf(stderr, "pid:%d\n", (int)getpid());
    printf("---\n");

    NetWork net;
    net.Listen(56789);
    while(true)
    {
        net.Loop();
    }

    return 0;
}




