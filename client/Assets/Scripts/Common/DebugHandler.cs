using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFKCore
{
    public class DebugHandler : ILogHandler
    {
        // LOG_FLUSH_SIZE长度写一次磁盘
        private const long LOG_FLUSH_SIZE = 1024 * 1024;
        // LOG_FLUSH_INTERAL秒写一次磁盘
        private const long LOG_FLUSH_INTERAL = 10000000;

        private ILogHandler unityLogHandler = null;
        private Thread writeThread = null;
        private bool writeThreadRuning = false;
        private DebugWriter writer = null;
        private LinkedList<string> backgroundLogList = new LinkedList<string>();
        private LinkedList<string> frontgroundLogList = new LinkedList<string>();

        public DebugHandler(ILogHandler unityLogHandler, string logFile)
        {
            this.writeThreadRuning = false;

            try
            {
                this.writer = new DebugWriter(logFile);
            }
            catch (Exception e)
            {
                Debug.LogWarning("文件操作失败 " + e.ToString());
                if (this.writer != null)
                {
                    this.writer.Dispose();
                }
                return;
            }

            this.unityLogHandler = unityLogHandler;

            this.writeThread = new Thread(this.WriteLogThread);
            this.writeThread.Start();
            this.writeThreadRuning = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.writeThreadRuning = false;

            if (disposing)
            {
                if (this.writer != null)
                {
                    this.writer.Dispose();
                }
                this.writer = null;

                if (this.writeThread != null)
                {
                    this.writeThread.Join();
                    // Join后，日志不输出到文件
                }

                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void WriteLogThread()
        {
            Debug.Log("start thread");
            long lastTime = DateTime.UtcNow.Ticks;

            while(this.writeThreadRuning)
            {
                if (this.backgroundLogList.Count <= 0)
                {
                    // 空间时，检查是否有日志未写磁盘
                    if (this.writer != null && this.writer.Length > 0)
                    {
                        long nowTime = DateTime.UtcNow.Ticks;
                        if (nowTime - lastTime > LOG_FLUSH_INTERAL)
                        {
                            // this.unityLogHandler.LogFormat(LogType.Log, null, "Flush as log thread idled {0}", this.writer.Length);
                            this.writer.Flush();

                            lastTime = nowTime;
                        }
                    }
                   continue;
                }

                lock(this.frontgroundLogList)
                {
                    var tmp = this.frontgroundLogList;
                    this.frontgroundLogList = this.backgroundLogList;
                    this.backgroundLogList = tmp;
                }

                var itr = this.frontgroundLogList.First;
                while (itr != null)
                {
                    this.writer.Write(itr.Value);
                    // this.unityLogHandler.LogFormat(LogType.Log, null, "len: {0}", this.writer.Length);
                    if (this.writer.Length > LOG_FLUSH_SIZE)
                    {
                        this.writer.Flush();
                        // this.unityLogHandler.LogFormat(LogType.Error, null, "len: {0}", this.writer.Length);
                    }

                    itr = itr.Next;
                }

                this.frontgroundLogList.Clear();
            }
            Debug.Log("end thread");
        }

        private void RawLogFormat(LogType logType, string format, params object[] args)
        {
            if (this.unityLogHandler != null)
            {
                this.unityLogHandler.LogFormat(logType, null, format, args);
            }
        }

        private void RawLogException(Exception exception, UnityEngine.Object context)
        {
            if (this.unityLogHandler != null)
            {
                this.unityLogHandler.LogException(exception, null);
            }

        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            this.unityLogHandler.LogFormat(logType, context, format, args);

            if (this.writeThreadRuning)
            {
                // StackFrame[] stacks = new StackTrace().GetFrames();
                // string result = str + "\r\n";

                // if (stacks != null)
                // {
                //     for (int i = 0; i < stacks.Length; i++)
                //     {
                //         result += string.Format("{0} {1}\r\n", stacks[i].GetFileName(), stacks[i].GetMethod().ToString());
                //         //result += stacks[i].ToString() + "\r\n";
                //     }
                // }
                lock(this.backgroundLogList)
                {
                    this.backgroundLogList.AddLast(string.Format(format, args));
                }
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            this.unityLogHandler.LogException(exception, context);

            if (this.writeThreadRuning)
            {
                lock (this.backgroundLogList)
                {
                    this.backgroundLogList.AddLast(exception.ToString());
                }
            }
        }
    }

    public static class DebugHelper
    {
        public static string LOG_FILE = Application.persistentDataPath + "/myLog.txt";

        private static ILogHandler _unityLogHdlr = null;
        private static UFKCore.DebugHandler hdlr = null;

        public static void Startup()
        {
            if (_unityLogHdlr == null)
            {
                _unityLogHdlr = Debug.unityLogger.logHandler;
            }

            if (hdlr == null)
            {
                hdlr = new UFKCore.DebugHandler(_unityLogHdlr, LOG_FILE);
            }

            // 如果已经设置
            if (Debug.unityLogger.logHandler == hdlr)
            {
                return;
            }

            Debug.unityLogger.logHandler = hdlr;
            Debug.Log("LOG_FILE: " + LOG_FILE);
        }

        public static void Release()
        {
            if (hdlr != null)
            {
                hdlr.Dispose();
            }
            hdlr = null;
        }

        public static bool logEnabled
        {
            get
            {
                return Debug.unityLogger.logEnabled;
            }

            set
            {
                Debug.unityLogger.logEnabled = value;
            }
        }
    }
}