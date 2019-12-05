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

        private LinkedList<string> backList = new LinkedList<string>();
        private LinkedList<string> frontList = new LinkedList<string>();

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
            Debug.LogFormat("logFile: {0}", logFile);

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
                if (this.backList.Count <= 0)
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

                lock(this.frontList)
                {
                    var tmp = this.frontList;
                    this.frontList = this.backList;
                    this.backList = tmp;
                }

                var itr = this.frontList.First;
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

                this.frontList.Clear();
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

        private void _Log(LogType logType, string format, params object[] args)
        {
            lock (this.backList)
            {
                System.Diagnostics.StackFrame[] stacks = new System.Diagnostics.StackTrace(1, true).GetFrames();
                if (stacks != null && stacks.Length > 0)
                {
                    System.Diagnostics.StackFrame frame = stacks[0];
                    string v1 = string.Format("{0} {1}:{2}", 
                                            logType, 
                                            frame.GetFileName(),
                                            frame.GetFileLineNumber());
                    string v2 = string.Format(format, args);
                    this.backList.AddLast(string.Format("{0} {1}",v1, v2));
                }
                else
                {
                    this.backList.AddLast(string.Format(format, args));
                }
            }
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (this.unityLogHandler != null)
            {
                this.unityLogHandler.LogFormat(logType, context, format, args);
            }

            if (this.writeThreadRuning)
            {
                this._Log(logType, format, args);
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (this.unityLogHandler != null)
            {
                this.unityLogHandler.LogException(exception, context);
            }

            if (this.writeThreadRuning)
            {
                lock (this.backList)
                {
                    this._Log(LogType.Exception, exception.ToString());
                }
            }
        }
    }

    public static class DebugHelper
    {
        private static ILogHandler unityHdl = null;
        private static UFKCore.DebugHandler ufkHdl = null;

        public static string LogFilePath
        {
            get; set;
        }

        public static void Startup()
        {
            // 缓存Unity内存处理实例
            if (unityHdl == null)
            {
                unityHdl = Debug.unityLogger.logHandler;
            }

            // 自定义日志处理实例
            if (ufkHdl == null)
            {
                if (string.IsNullOrEmpty(LogFilePath))
                {
                    LogFilePath = GenDefaultLogFilePath();
                }
                ufkHdl = new UFKCore.DebugHandler(unityHdl, LogFilePath);
            }

            // 如果已经设置
            if (Debug.unityLogger.logHandler == ufkHdl)
            {
                return;
            }

            Debug.unityLogger.logHandler = ufkHdl;
        }

        public static string GenDefaultLogFilePath()
        {
            DateTime now = DateTime.Now;
            return string.Format("{0}/{1}/{2}.log", 
                        Application.persistentDataPath, 
                        now.ToString("MMdd"), 
                        now.ToString("HHmmss"));
        }

        public static void Release()
        {
            if (ufkHdl != null)
            {
                ufkHdl.Dispose();
            }
            ufkHdl = null;
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