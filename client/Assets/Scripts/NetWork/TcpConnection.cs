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


public class TCPConnection : TcpClient
{
    private class SocketStateChangedEventArgs
    {
        public SocketState state;
        public string msg;
    }

    public enum SocketState
    {
        NONE,
        CONNECTED,
        CONNECTING,
        CLOSED,
        ERROR,
        TIME_OUT,
    }



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

    public SocketState State = SocketState.NONE;   //current network state

    protected SocketStateChangeDelegate SocketStateChangedEvent;

    public delegate void SocketStateChangeDelegate(SocketState state, string msg);
    private Queue<SocketStateChangedEventArgs> socketChangeQueue = new Queue<SocketStateChangedEventArgs>();
    private Queue<SocketStateChangedEventArgs> socketChangeBuffer = new Queue<SocketStateChangedEventArgs>();
    //

    public TCPConnection() {
    }

    public virtual void Connect (string adr, int port, System.Action cb) 
    {
        if (State == SocketState.CONNECTING)
        {
            // 等待上次连接操作结束
            Debug.LogWarning("please waiting for the last connting");
            return;
        }

        this.Dispose ();

        ip = adr;   //保存地址
        this.port = port;
   
        if (threadConnect == null) 
        {
            threadConnect = new Thread(CbConnectCommon);
            threadConnect.IsBackground = true;
        }

        threadConnect.Start();

        this.SocketStateChanged(SocketState.CONNECTING, "");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposin) {

        this.SocketStateChanged(SocketState.CLOSED, "");

        if (threadConnect != null && threadConnect.IsAlive)
        {
            threadConnect.Abort ();
        }

        if (this.Connected)
        {
            try {
                this.Close ();
                if (networkStream != null)
                {
                    networkStream.Flush();
                    networkStream.Close ();
                }
            } catch (Exception) {
                Debug.LogWarning ("TCPSocket Disconnect error");
            }
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
    }



    private void ThreadConnectTimeOut(System.Object source, ElapsedEventArgs e)
    {
        if (!this.Connected)
        {
            this.SocketStateChanged(SocketState.TIME_OUT, "connection timed out");
            this.Dispose();
        }
    }

    private void CbConnectCommon()
    {
        Debug.Log("CbConnectCommon");
        IPAddress[] addrs = Dns.GetHostAddresses(ip);
        if (addrs.Length == 0)
        {
            Debug.LogError("[CbConenctCommon] GetHostAddressess retuan value the Length is Zero!");
            return;
        }

        timeoutEvent.Reset();
        this.BeginConnect(addrs, port, new AsyncCallback((result) =>
        {
            try
            {
                this.EndConnect(result);
                networkStream = this.GetStream();
                this.SocketStateChanged(SocketState.CONNECTED, "");
            }
            catch (SocketException e)
            {
                Debug.Log("ConnectThread->Connect Failed :" + ip + ":" + port + e.ToString());
                if (State != SocketState.TIME_OUT)
                {
                    this.SocketStateChanged(SocketState.ERROR, "ConnectThread->Connect to Server fail.\n");
                }
            }
            finally
            {
                timeoutEvent.Set();
            }


        }), this);

        if (timeoutEvent.WaitOne(CONNECT_TIME_OUT, false))
        {
            if (State != SocketState.CONNECTED && State != SocketState.ERROR)
            {
                this.SocketStateChanged(SocketState.TIME_OUT, "connect time out");
                this.Dispose();
                return;
            }
        }

        threadRead = new Thread(ThreadRead);
        threadRead.IsBackground = true;
        threadRead.Start();

        // send thread
        threadSend = new Thread(ThreadSend);
        threadRead.IsBackground = true;
        threadRead.Start();
    }

    private void ThreadSend()
    {
        while(true)
        {
            if (State != SocketState.CONNECTED)
            {
                this.SocketStateChanged(SocketState.ERROR, "TCPSocket Read-> socket DisconnecteDebug.");
                break;
            }

            if (sendQueue.Count <= 0)
            {
                continue;
            }

            if (networkStream.CanWrite)
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
                }
                catch (Exception e)
                {
                    this.SocketStateChanged(SocketState.ERROR, "TCPSocket Send->\n" + e);
                }
            }
        }
    }

    private void ThreadRead () {
        MemoryStream memoryStream;
        NetworkStream stream = networkStream;
        byte[] arrByte;
        while (true) {
            if (State != SocketState.CONNECTED || !this.Connected) {
                this.SocketStateChanged(SocketState.ERROR, "TCPSocket Read-> socket DisconnecteDebug.");
                break;
            }

            if (stream.CanRead)
            {
                try
                {
                    //TODO:
                }
                catch (Exception e)
                {
                    Debug.Log("TcpSocket Read" + e.ToString());
                    return;    
                } 
               
            } else {
                this.SocketStateChanged(SocketState.ERROR, "TCPSocket Read->networkStream can not be reaDebug.");
                break;
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
            {
                SocketStateChangedEvent(args.state, args.msg);
            }

            this.FreeEventArgs(args);
        }
    }


    protected void SocketStateChanged(SocketState state, string msg)
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
        if (state == SocketState.ERROR)
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

            changeMsg.state = state;
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
            args.state = SocketState.NONE;
            args.msg = null;
            this.socketChangeBuffer.Enqueue(args);
        }
    }

    public virtual void Send(byte[] data)
    {
        if (this.Connected)
        {
            lock (sendQueue)
            {
                sendQueue.Enqueue(data);
            }
        }
    }

    public virtual void Receive(byte[] data)
    {
        throw new NotImplementedException();
    }
}