#include <stdio.h>
#include <unistd.h>

#include "application/Application.h"

int
main(int argc, char** argv)
{
    printf("---\n");
    fprintf(stderr, "pid:%d\n", (int)getpid());
    printf("---\n");

    Application app;
    app.Start();
    return 0;
}


