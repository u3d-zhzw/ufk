using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Timers;

using Debug=UnityEngine.Debug;

#if UNITY_EDITOR
using System.Diagnostics;
#endif


public enum NetStatus
{
    NONE,
    CONNECTED,
    CONNECTING,
    CLOSED,
    ERROR,
    TIME_OUT,
}

public class TCPConnection : TcpClient
{
    private class SocketStateChangedEventArgs
    {
        public NetStatus status;
        public string msg;
    }

    public delegate void SocketStateChangeDelegate(NetStatus state, string msg);

    // 10秒超时连接
    private const int CONNECT_TIME_OUT = 8000;
    private Queue<byte[]> sendQueue = new Queue<byte[]>();

    private Thread threadSend;
    private Thread threadRead;
    private Thread threadConnect;

    private ManualResetEvent timeoutEvent = new ManualResetEvent(false);

    //
    private int port;
    private string ip = "";
    private NetworkStream networkStream;

    public NetStatus State = NetStatus.NONE;   //current network state

    public SocketStateChangeDelegate SocketStateChangedEvent;

    private Queue<SocketStateChangedEventArgs> socketChangeQueue = new Queue<SocketStateChangedEventArgs>();
    private Queue<SocketStateChangedEventArgs> socketChangeBuffer = new Queue<SocketStateChangedEventArgs>();
    //

    public bool Working
    {
        get
        {
            return State == NetStatus.CONNECTED && this.Connected;
        }
    }

    public TCPConnection()
        : base()
    {
    }

    public new void Connect(string ip, int port) 
    {
        if (State == NetStatus.CONNECTING)
        {
            // 等待上次连接操作结束
            Debug.LogWarning("please waiting for the last connting");
            return;
        }

        this.ip = ip;   //保存地址
        this.port = port;
        
        if (this.threadConnect == null) 
        {
            this.threadConnect = new Thread(ThreadConnectCommon);
            this.threadConnect.IsBackground = true;
        }

        this.threadConnect.Start();
        this.SocketStateChanged(NetStatus.CONNECTING, "");
    }

    public void Disconnect()
    {
        Debug.LogError("disconnect");
        this.Close();
    }

    protected override void Dispose(bool disposing)
    {
        this.SocketStateChanged(NetStatus.CLOSED, "");

        if (threadConnect != null && threadConnect.IsAlive)
        {
            threadConnect.Abort ();
        }

        if (threadSend != null && threadRead.IsAlive)
        {
            threadSend.Join();
            Debug.Log("threadRead.Abort");
        }

        if (threadRead != null && threadRead.IsAlive)
        {
            threadRead.Join();
            Debug.Log("threadRead.Abort");
        }

        this.threadConnect = null;
        this.threadSend = null; 
        this.threadRead = null;
        this.networkStream = null;

        base.Dispose(disposing);
    }

    private void ThreadConnectTimeOut(System.Object source, ElapsedEventArgs e)
    {
        if (!this.Connected)
        {
            this.SocketStateChanged(NetStatus.TIME_OUT, "connection timed out");
            this.Disconnect();
        }
    }

    private void ThreadConnectCommon()
    {
        Debug.Log("CbConnectCommon");
        IPAddress[] addrs = Dns.GetHostAddresses(ip);
        if (addrs.Length == 0)
        {
            Debug.LogError("[CbConenctCommon] GetHostAddressess retuan value the Length is Zero!");
            return;
        }

        // support for ipv4 & ipv6
        timeoutEvent.Reset();
        this.BeginConnect(addrs, port, new AsyncCallback((result) =>
        {
            try
            {
                this.EndConnect(result);
                networkStream = this.GetStream();
                this.SocketStateChanged(NetStatus.CONNECTED, "");
            }
            catch (SocketException e)
            {
                Debug.Log("ConnectThread->Connect Failed :" + ip + ":" + port + e.ToString());
                if (State != NetStatus.TIME_OUT)
                {
                    this.SocketStateChanged(NetStatus.ERROR, "ConnectThread->Connect to Server fail.\n");
                }
            }
            finally
            {
                timeoutEvent.Set();
            }


        }), this);

        // check if connect connect time
        timeoutEvent.WaitOne(CONNECT_TIME_OUT, false);
        if (State != NetStatus.CONNECTED && State != NetStatus.ERROR)
        {
            this.SocketStateChanged(NetStatus.TIME_OUT, "connect time out");
            this.Disconnect();
            return;
        }

        if (!Working)
        {
            this.Disconnect();
            return;
        }

        threadRead = new Thread(ThreadRead);
        threadRead.IsBackground = true;
        threadRead.Start();

        threadSend = new Thread(ThreadSend);
        threadSend.IsBackground = true;
        threadSend.Start();
    }

    private void ThreadSend()
    {
        while(this.Working)
        {
            if (networkStream != null && networkStream.CanWrite 
                && sendQueue != null &&  sendQueue.Count > 0)
            {
                byte[] data = sendQueue.Dequeue();
                if (data == null || data.Length <= 0)
                {
                    continue;
                }

                try
                {
                    networkStream = this.GetStream();
                    networkStream.Write(data, 0, data.Length);
                    networkStream.Flush();

                    Debug.Log("send done len:" + data.Length);
                }
                catch (Exception e)
                {
                    this.SocketStateChanged(NetStatus.ERROR, "TCPSocket Send->\n" + e);
                }
            }
        }
    }

    private void ThreadRead ()
    {
        while (this.Working)
        {
            if (networkStream != null
                && networkStream.CanRead
                && networkStream.DataAvailable)
            {
                try
                {
                    this.Receive();
                }
                catch (Exception e)
                {
                    this.SocketStateChanged(NetStatus.ERROR, "TCPSocket Read->\n" + e);
                    return;
                }
            }
        }
    }


    public virtual void Update()
    {
        while (socketChangeQueue.Count > 0)
        {
            SocketStateChangedEventArgs args = null;
            lock (socketChangeQueue)
            {
                args = socketChangeQueue.Dequeue();
            }

            if (SocketStateChangedEvent != null)
                SocketStateChangedEvent(args.status, args.msg);
            

            this.FreeEventArgs(args);
        }
    }

    protected void SocketStateChanged(NetStatus state, string msg)
    {
        State = state;

#if UNITY_EDITOR
        /**
         * 由于事件切回unity主线程处理后，无法跟踪调用栈。
         * 这里_DEBUG条件，从socket线程环境下打印数据
        */
        StackTrace st = new StackTrace(true);
        string stackIndent = "\n 触发网络状态:";
        stackIndent += "state: " + state + "\tmsg: " + msg + "\n";
        stackIndent += "begin----------------\n";

        for (int i = 0; i < st.FrameCount; i++)
        {
            StackFrame sf = st.GetFrame(i);
            stackIndent += string.Format("{0} ({1} line:{2})\n",
                sf.GetMethod(), sf.GetFileName(), sf.GetFileLineNumber());
        }
        stackIndent += "end----------------\n";
        if (state == NetStatus.ERROR)
            UnityEngine.Debug.LogError(stackIndent);
        else
            UnityEngine.Debug.LogWarning(stackIndent);
#endif

        lock (this.socketChangeQueue)
        {
            SocketStateChangedEventArgs changeMsg = this.AllocEventArgs();
            if (changeMsg == null)
            {
                UnityEngine.Debug.LogWarning("alloc SocketChangeMsg fail");
                return;
            }

            changeMsg.status = state;
            changeMsg.msg = msg;

            this.socketChangeQueue.Enqueue(changeMsg);
        }
    }

    private SocketStateChangedEventArgs AllocEventArgs()
    {
        SocketStateChangedEventArgs args = null;

        lock (this.socketChangeBuffer)
        {
            if (this.socketChangeBuffer.Count <= 0)
            {
                args = new SocketStateChangedEventArgs();
            }
            else
            {
                args = this.socketChangeBuffer.Dequeue();
            }
        }

        return args;
    }

    private void FreeEventArgs(SocketStateChangedEventArgs args)
    {
        lock (this.socketChangeBuffer)
        {
            args.status = NetStatus.NONE;
            args.msg = null;
            this.socketChangeBuffer.Enqueue(args);
        }
    }

    public void Send(byte[] data)
    {
        if (this.Connected)
        {
            lock (sendQueue)
            {
                sendQueue.Enqueue(data);
            }
        }
    }

    public virtual void Receive()
    {
        throw new NotImplementedException();
    }
}
