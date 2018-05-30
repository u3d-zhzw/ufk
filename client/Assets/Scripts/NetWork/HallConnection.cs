using UnityEngine;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;

class HallConnection
    : TCPConnection
{
    public const ushort PACKET_HEAD_SIZE = 7;
    // body数据在头部偏移量的位置
    public const ushort BODY_SIZE_OFFSET = 3;

    public class Protocol
    {
        public ushort id;
        public byte[] data;
    }

    public delegate void ProtocolResponseHandler(ushort id, byte[] data);

    private System.Action cbConn = null;
    private System.Action<bool> cbReconn = null;

    private Dictionary<ushort, LinkedList<ProtocolResponseHandler>> dictProtocol = new Dictionary<ushort, LinkedList<ProtocolResponseHandler>>();

    private Queue<Protocol> protoQueue = new Queue<Protocol>();
    byte[] pkgBuffer = new byte[1024];
    private ushort remainPkgSize = 0;

    public void Connect(string ip, int port, System.Action cbConn = null, System.Action<bool> cbReconn = null)
    {
        this.cbConn = cbConn;
        this.cbReconn = cbReconn;

        base.SocketStateChangedEvent -= this.OnSocketStatusEvent;
        base.SocketStateChangedEvent += this.OnSocketStatusEvent;
        this.ReceiveBufferSize = (int)PACKET_HEAD_SIZE;
        this.remainPkgSize = 0;

        base.Connect(ip, port);
    }

    private void OnSocketStatusEvent(NetStatus sst, string msg) 
    {
        if (sst == NetStatus.CONNECTING)
        {

        }
        else if (sst == NetStatus.CONNECTED)
        {
            if (this.cbConn != null)
            {
                this.cbConn();
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

    public void Send<T>(ushort id, T t)
    {
        byte[] bodyBuf = null;
        using (MemoryStream memStream = new MemoryStream())
        {
            Serializer.Serialize(memStream, t);
            bodyBuf = memStream.ToArray();
        }

        ushort bodySize = 0;
        ushort pkgSize = (ushort)PACKET_HEAD_SIZE;
        if (bodyBuf != null && bodyBuf.Length > 0)
        {
            bodySize = (ushort)bodyBuf.Length;
        }
        pkgSize += bodySize;

        byte[] pkgBuf = null;
        using (MemoryStream memStream = new MemoryStream())
        {
            using(BinaryWriter writer = new BinaryWriter(memStream))
            {
                id = (ushort)IPAddress.HostToNetworkOrder((short)id);
                bodySize = (ushort)IPAddress.HostToNetworkOrder((short)bodySize);
                pkgSize = (ushort)IPAddress.HostToNetworkOrder((short)pkgSize);

                writer.Write((byte)1);
                writer.Write(id);
                writer.Write(bodySize);
                writer.Write(pkgSize);
                if (pkgSize > bodySize)
                {
                    writer.Write(bodyBuf, 0, bodyBuf.Length);
                }
            }

            pkgBuf = memStream.ToArray();
        }

        base.Send(pkgBuf);
    }

    public void Listen(ushort id, ProtocolResponseHandler handler)
    {
        if (!this.dictProtocol.ContainsKey(id))
        {
            this.dictProtocol.Add(id, new LinkedList<ProtocolResponseHandler>());
        }

        this.dictProtocol[id].AddLast(handler);
    }

    public void UnListen(ushort id, ProtocolResponseHandler handler)
    {
        if (!this.dictProtocol.ContainsKey(id))
        {
            return;
        }
        this.dictProtocol[id].Remove(handler);
    }

    public void ResetListen(ushort id)
    {
        if (!this.dictProtocol.ContainsKey(id))
        {
            return;
        }

        this.dictProtocol[id].Clear();
    }

    private Protocol AllocProtocol()
    {
        // todo reuse
        Protocol p = new Protocol();
        return p;
    }

    private void FreeProtocol(Protocol p)
    {
        // todo reuse
        p = null;
    }

    public override void Update()
    {
        base.Update();

        Queue<Protocol> frameQue = null;
        lock (this.protoQueue)
        {
            frameQue = new Queue<Protocol>(this.protoQueue);
        }

        while(frameQue.Count > 0)
        {
            Protocol p = frameQue.Dequeue();
           
        }
    }

    public override void Receive()
    {
        NetworkStream stream = this.GetStream();

        if (this.Available >= PACKET_HEAD_SIZE && this.remainPkgSize <= 0)
        {
            stream.Read(pkgBuffer, 0, (int)PACKET_HEAD_SIZE);
            this.remainPkgSize  = BitConverter.ToUInt16(pkgBuffer, (int)BODY_SIZE_OFFSET);
            this.remainPkgSize = (ushort)IPAddress.NetworkToHostOrder((short)this.remainPkgSize);
            this.ReceiveBufferSize = this.remainPkgSize;
        }

        // 
        if (this.remainPkgSize > 0 && this.remainPkgSize <= this.Available)
        {
            stream.Read(pkgBuffer, PACKET_HEAD_SIZE, this.remainPkgSize);

            byte type = pkgBuffer[0];
            ushort id = BitConverter.ToUInt16(pkgBuffer, 1);
            ushort bodySize = BitConverter.ToUInt16(pkgBuffer, 3);
            ushort pkgSize = BitConverter.ToUInt16(pkgBuffer, 5);

            id = (ushort)IPAddress.HostToNetworkOrder((short)id);
            bodySize = (ushort)IPAddress.HostToNetworkOrder((short)bodySize);
            pkgSize = (ushort)IPAddress.HostToNetworkOrder((short)pkgSize);

            // todo check crc
            byte[] bodyData = new byte[bodySize];
            Array.Copy(pkgBuffer, PACKET_HEAD_SIZE, bodyData, 0, bodySize);

            Protocol p = AllocProtocol();
            p.id = id;
            p.data = bodyData;

            this.remainPkgSize = 0;
            this.protoQueue.Enqueue(p);
            this.ReceiveBufferSize = PACKET_HEAD_SIZE; 

            if (this.Available > 0)
            {
                this.Receive();
            }
        }
    }
}
