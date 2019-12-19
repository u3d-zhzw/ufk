using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFK
{
    public static class DebugUtils
    {
        // LOG_FLUSH_SIZE长度写一次磁盘
        private const long LOG_FLUSH_SIZE = 1024 * 1024;
        // LOG_FLUSH_INTERAL秒写一次磁盘
        private const long LOG_FLUSH_INTERAL = 10000000;

        private static DebugWriter s_writer = null;
        private static System.Object s_lockFlag = new System.Object();

        public static string LogFilePath { get; set; }

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

        public static void Startup()
        {
            // 自定义日志处理实例
            if (string.IsNullOrEmpty(LogFilePath))
            {
                LogFilePath = GenDefaultLogFilePath();
            }

            try
            {
                s_writer = new DebugWriter(LogFilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("文件操作失败 " + e.ToString());
                if (s_writer != null)
                {
                    s_writer.Dispose();
                }
                return;
            }

            Application.logMessageReceived -= LogCallback;
            Application.logMessageReceived += LogCallback;
            Application.logMessageReceivedThreaded -= LogCallback;
            Application.logMessageReceivedThreaded += LogCallback;
        }

        public static void Release()
        {
            if (s_writer != null)
            {
                s_writer.Dispose();
            }
            s_writer = null;
        }

        private static string GenDefaultLogFilePath()
        {
            DateTime now = DateTime.Now;
            return string.Format("{0}/{1}/{2}.log",
                        Application.persistentDataPath,
                        now.ToString("MMdd"),
                        now.ToString("HHmmss"));
        }

        private static void LogCallback(string condition, string stackTrace, LogType type)
        {
            lock (s_lockFlag)
            {
                s_writer.Write(condition, stackTrace, type);
                s_writer.Flush();
            }
        }
    }
}