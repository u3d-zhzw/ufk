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


public enum SocketState
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
        public SocketState state;
        public string msg;
    }

    private class SocketStateListenerArgs
    {
        public SocketState state;
        public bool once;
        public SocketStateChangeDelegate del;
        public System.Object userdata;
    }

    public delegate void SocketStateChangeDelegate(SocketState state, string msg, System.Object userdata);


    // 10秒超时连接
    private const int CONNECT_TIME_OUT = 8000;
    private Queue<byte[]> sendQueue = new Queue<byte[]>();

    private Thread threadSend;
    private Thread threadRead;
    private Thread threadConnect;
    private bool active;

    private ManualResetEvent timeoutEvent = new ManualResetEvent(false);

    //
    private int port;
    private string ip = "";
    private NetworkStream networkStream;

    public SocketState State = SocketState.NONE;   //current network state

    public SocketStateChangeDelegate SocketStateChangedEvent;

    Dictionary<SocketState, LinkedList<SocketStateListenerArgs>> dictNetStateListener = new Dictionary<SocketState, LinkedList<SocketStateListenerArgs>>();

    public void AddStatusListener(SocketState st, SocketStateChangeDelegate cb, System.Object userdata = null)
    {
        this.AddStatusListener(st, false, cb, userdata);
    }

    public void AddStatusListener(SocketState st, bool once, SocketStateChangeDelegate cb, System.Object userdata = null)
    {
        // todo: pool
        SocketStateListenerArgs arg = AllocStatusArgs();
        arg.state = st;
        arg.once = once;
        arg.userdata = userdata;
        arg.del = cb;

        LinkedList<SocketStateListenerArgs> list = null;
        if(!this.dictNetStateListener.ContainsKey(st))
        {
            list = new LinkedList<SocketStateListenerArgs>();
            this.dictNetStateListener[st] = list;
        }

        list.AddLast(arg);
    }

    private SocketStateListenerArgs AllocStatusArgs()
    {
        return new SocketStateListenerArgs();
    }

    private void FreeStatusArgs(SocketStateListenerArgs args)
    {

    }

    private Queue<SocketStateChangedEventArgs> socketChangeQueue = new Queue<SocketStateChangedEventArgs>();
    private Queue<SocketStateChangedEventArgs> socketChangeBuffer = new Queue<SocketStateChangedEventArgs>();
    //

    public TCPConnection()
        : base()
    {
    }

    public virtual void Connect (string adr, int port, SocketStateChangeDelegate callback = null, System.Object userdata = null) 
    {
        if (State == SocketState.CONNECTING)
        {
            // 等待上次连接操作结束
            Debug.LogWarning("please waiting for the last connting");
            return;
        }

        this.AddStatusListener(SocketState.CONNECTED, true, callback, null);

        this.ip = adr;   //保存地址
        this.port = port;
        this.active = true;
        
        if (threadConnect == null) 
        {
            threadConnect = new Thread(ThreadConnectCommon);
            threadConnect.IsBackground = true;
        }

        threadConnect.Start();
        this.SocketStateChanged(SocketState.CONNECTING, "");
    }

    public void Disconnect()
    {
        Debug.LogError("disconnect");
        this.Close();
    }

    protected override void Dispose(bool disposing)
    {
        this.active = false;

        this.SocketStateChanged(SocketState.CLOSED, "");

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
            this.SocketStateChanged(SocketState.TIME_OUT, "connection timed out");
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

        timeoutEvent.Reset();
        this.BeginConnect(addrs, port, new AsyncCallback((result) =>
        {
            try
            {
                if (!result.IsCompleted)
                {
                    
                }

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

        timeoutEvent.WaitOne(CONNECT_TIME_OUT, false);

        if (State != SocketState.CONNECTED && State != SocketState.ERROR)
        {
            this.SocketStateChanged(SocketState.TIME_OUT, "connect time out");
            this.Disconnect();
            return;
        }

        if (!this.active)
        {
            this.Disconnect();
            return; 
        }

        threadRead = new Thread(ThreadRead);
        threadRead.IsBackground = true;
        threadRead.Start();

        // send thread
        threadSend = new Thread(ThreadSend);
        threadSend.IsBackground = true;
        threadSend.Start();
    }

    private void ThreadSend()
    {
        while(this.active)
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

                    Debug.Log("send done len:" + data.Length);
                }
                catch (Exception e)
                {
                    this.SocketStateChanged(SocketState.ERROR, "TCPSocket Send->\n" + e);
                }
            }
        }
    }

    private void ThreadRead () {
        while (this.active) {
            if (State != SocketState.CONNECTED || !this.Connected) {
                this.SocketStateChanged(SocketState.ERROR, "TCPSocket Read-> socket DisconnecteDebug.");
                break;
            }

            if (networkStream != null 
                && networkStream.CanRead 
                && networkStream.DataAvailable)
            {
                    try
                    {
                        byte[] data = new byte[128];
                        int len = networkStream.Read(data, 0, 128);
                        Debug.Log("rec data len = " + len);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("TcpSocket Read" + e.ToString());
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

            if (this.dictNetStateListener.ContainsKey(args.state))
            {
                LinkedList<SocketStateListenerArgs> list = this.dictNetStateListener[args.state];
                Debug.LogError("before " + list.Count);
                LinkedListNode<SocketStateListenerArgs> itr = list.First;
                while (itr != null)
                {
                    SocketStateChangeDelegate del = itr.Value.del;
                    if (del != null)
                    {
                        del(args.state, args.msg, itr.Value.userdata);
                    }

                    if (itr.Value.once)
                    {
                        list.Remove(itr);
                    }
                    itr = itr.Next;

                }

                Debug.LogError("after " + list.Count);
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