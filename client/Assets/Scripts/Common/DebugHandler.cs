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
        private ILogHandler unityLogHandler = null;
        private Thread writeThread = null;
        private bool writeThreadRuning = false;
        private StreamWriter logWriter = null;
        private LinkedList<string> backgroundLogList = new LinkedList<string>();
        private LinkedList<string> frontgroundLogList = new LinkedList<string>();

        public DebugHandler(ILogHandler unityLogHandler, string logFile)
        {
            try
            {
                this.logWriter = new StreamWriter(logFile, true, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning("文件操作失败 " + e.ToString());
                return;
            }
            finally
            {
                if (logWriter != null)
                {
                    logWriter.Dispose();
                }
            }

                // lock (sw)
                // {
                //     //开始写入
                //     sw.WriteLine(msg);
                //     //清空缓冲区
                //     sw.Flush();
                //     //关闭流
                //     sw.Close();
                // }

            this.writeThread = new Thread(this.WriteLogThread);
            this.writeThreadRuning = true;

            this.unityLogHandler = unityLogHandler;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.writeThreadRuning = false;

            if (disposing)
            {
                if (this.logWriter != null)
                {
                    this.logWriter.Dispose();
                }
                this.logWriter = null;

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
            while(this.writeThreadRuning && Application.isPlaying)
            {
                
            }
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_EDITOR
            this.unityLogHandler.LogFormat(logType, context, format, args);
#endif
            if (this.writeThreadRuning)
            {
                // todo: 
                // 1. 字符池尝试使用缓存池
                // 2. 分类Tag
                // 3. 无锁队列
                
                this.backgroundLogList.AddLast(string.Format(format, args));
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            this.unityLogHandler.LogException(exception, context);
#endif
            if (this.writeThreadRuning)
            {
                // todo: 
                // 1. 字符池尝试使用缓存池
                // 2. 分类Tag
                // 3. 无锁队列
                this.backgroundLogList.AddLast(exception.ToString());
            }
        }
    }

    public static class DebugHelper
    {
        public static void Startup()
        {
            // Debug.unityLogger.logHandler = new UFKCore.DebugHandler(Debug.unityLogger.logHandler);
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