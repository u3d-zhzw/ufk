using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class NetPacket
{
    struct PacketHead
    {
        public byte main;
        public byte sub;
        public uint size;
    }

    public const uint PACKET_SIZE_OFFSET = 16;
    public const uint PACKET_SIZE = 48;

    public NetPacket(byte[] buffer)
    {

    }





}
