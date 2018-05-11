using UnityEngine;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

class HallConnection
    : TCPConnection
{

    public delegate void NetPacketHandlerEvent(NetPacket pkg);

    private System.Action<System.Object> tmpCallback = null;
    private System.Object tmpuserData = null;

    public event NetPacketHandlerEvent NetPacketHander = null;

    private uint remainSize = 0;
    private byte[] packetBuffer = null;


    public void Connect(string ip, int port, System.Action<System.Object> successCallback = null, System.Object userdata = null)
    {
        this.tmpCallback = successCallback;
        this.tmpuserData = userdata;

        base.SocketStateChangedEvent -= this.OnSocketStatusEvent;
        base.SocketStateChangedEvent += this.OnSocketStatusEvent;

        base.Connect(ip, port);
    }

    private void OnSocketStatusEvent(NetStatus sst, string msg) 
    {
        if (sst == NetStatus.CONNECTING)
        {

        }
        else if (sst == NetStatus.CONNECTED)
        {
            if (tmpCallback != null)
            {
                tmpCallback(tmpuserData);
            }
        }
        else if (sst == NetStatus.CLOSED)
        {

        }
        else if (sst == NetStatus.TIME_OUT)
        {

        }
        else if (sst == NetStatus.ERROR)
        {

        }
        else
        {
            Debug.LogWarning("unkow net status");
        }
    }

    public void Send<T>(T t)
    {
        byte[] data = null;
        using (MemoryStream memStream = new MemoryStream())
        {
            Serializer.Serialize(memStream, t);
            data = memStream.ToArray();
        }

        if (data == null || data.Length <= 0)
        {
            Debug.LogWarning("emptye package");
            return;
        }

        base.Send(data);
    }

    public override void Receive(NetworkStream stream)
    {
        long len = stream.Length;
        if (len < NetPacket.PACKET_SIZE)
        {
            return;
        }

        byte[] headBuffer = new byte[NetPacket.PACKET_SIZE];
        int readSize = stream.Read(headBuffer, 0, (int)NetPacket.PACKET_SIZE);
        if (readSize < NetPacket.PACKET_SIZE)
        {
            throw new Exception("包头长度错误");
        }

        uint packSize = BitConverter.ToUInt32(headBuffer, (int)NetPacket.PACKET_SIZE_OFFSET);
        this.remainSize = packSize - NetPacket.PACKET_SIZE;
        if (stream.DataAvailable && this.remainSize > stream.Length)
        {
            return;
        }

        
        
        


    }
}
