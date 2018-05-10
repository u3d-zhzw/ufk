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
    private System.Action<System.Object> tmpCallback = null;
    private System.Object tmpuserData = null;

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
        Debug.Log("rec msg");
    }
}
