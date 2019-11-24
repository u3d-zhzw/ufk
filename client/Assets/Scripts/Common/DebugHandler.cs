﻿using System;
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
        private DebugWriter dbWriter = null;
        private LinkedList<string> backgroundLogList = new LinkedList<string>();
        private LinkedList<string> frontgroundLogList = new LinkedList<string>();

        public DebugHandler(ILogHandler unityLogHandler, string logFile)
        {
            try
            {
                this.dbWriter = new DebugWriter(logFile);
            }
            catch (Exception e)
            {
                Debug.LogWarning("文件操作失败 " + e.ToString());
                if (this.dbWriter != null)
                {
                    this.dbWriter.Dispose();
                }
                return;
            }
            // finally
            // {
            //     if (dbWriter != null)
            //     {
            //         dbWriter.Dispose();
            //     }
            //     Debug.Log("dispose");
            // }

            this.unityLogHandler = unityLogHandler;

            this.writeThreadRuning = true;
            this.writeThread = new Thread(this.WriteLogThread);
            this.writeThread.Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            this.writeThreadRuning = false;

            if (disposing)
            {
                if (this.dbWriter != null)
                {
                    this.dbWriter.Dispose();
                }
                this.dbWriter = null;

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
            while(this.writeThreadRuning)
            {
                if (this.backgroundLogList.Count <= 0)
                {
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
                    this.dbWriter.Write(itr.Value);
                    itr = itr.Next;
                }

                this.frontgroundLogList.Clear();
            }
            Debug.Log("end thread");
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_EDITOR
            this.unityLogHandler.LogFormat(logType, context, format, args);
#endif
            if (this.writeThreadRuning)
            {
                lock(this.backgroundLogList)
                {
                    this.backgroundLogList.AddLast(string.Format(format, args));
                }
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            this.unityLogHandler.LogException(exception, context);
#endif
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