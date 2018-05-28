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
    public const uint PACKET_HEAD_SIZE = 7;
    // body数据在头部偏移量的位置
    public const uint PACKET_SIZE_OFFSET = 5;

    public class Protocol
    {
        public ushort id;
        public byte[] data;
    }

    public delegate void ProtocolResponseHandler(ushort id, byte[] data);

    private System.Action<System.Object> tmpCallback = null;
    private System.Object tmpuserData = null;

    public event ProtocolResponseHandler NetPacketHander = null;
    private Dictionary<ushort, LinkedList<ProtocolResponseHandler>> dictProtocol = new Dictionary<ushort, LinkedList<ProtocolResponseHandler>>();

    private Queue<Protocol> protoQueue = new Queue<Protocol>();
    private MemoryStream buffer = new MemoryStream();
    private uint pkgSize = 0;

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

        byte[] data = new byte[this.Available];
        int size = stream.Read(data, 0, this.Available);

        this.buffer.Seek(0, SeekOrigin.End);
        this.buffer.Write(data, 0, size);


        if (this.buffer.Length >= PACKET_HEAD_SIZE && this.pkgSize <= 0)
        {
            this.buffer.Seek(PACKET_SIZE_OFFSET, SeekOrigin.Begin);

            byte[] pkgSizeData = new byte[2];
            this.buffer.Read(pkgSizeData, (int)PACKET_SIZE_OFFSET, pkgSizeData.Length);
            this.pkgSize = BitConverter.ToUInt16(pkgSizeData, 0);

            this.buffer.Seek(0, SeekOrigin.End);
        }

        // 
        if (this.pkgSize > 0 && this.pkgSize < this.buffer.Length)
        {
            using (BinaryReader reader = new BinaryReader(this.buffer))
            {
                byte type = reader.ReadByte();
                ushort id = reader.ReadUInt16();
                ushort bodySize = reader.ReadUInt16();
                ushort pkgSize = reader.ReadUInt16();

                // todo check crc
                byte[] bodyData = new byte[bodySize];
                reader.Read(bodyData, 0, bodySize);

                Protocol p = AllocProtocol();
                p.id = id;
                p.data = bodyData;

                this.pkgSize = 0;
                this.protoQueue.Enqueue(p);


                // 处理剩余的是字节流
                long surplusSize = this.buffer.Length - pkgSize;
                byte[] surplusData = new byte[surplusSize];
                this.buffer.Seek(pkgSize, SeekOrigin.Begin);
                this.buffer.Read(surplusData, 0, (int)surplusSize);

                this.buffer.Seek(0, SeekOrigin.Begin);
                this.buffer.SetLength(0);
                this.buffer.Write(surplusData, 0, (int)surplusSize);

                this.Receive();
            }
        }
    }
}
