#include <stdio.h>

#include "common/Timer.h" 
#include "application/Application.h"

Application app;

void RandPacket()
{
    printf("sdfsdf\n");
app.Send();
}

int
main()
{
    Timer t;
    t.create(0, 1000, RandPacket);

    app.Star();
    getchar();
    return 0;
}
