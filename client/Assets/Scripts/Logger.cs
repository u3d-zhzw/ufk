using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// todo: 
/// 1. 控制台日志
/// 2. 文件日志
/// </summary>
public class Logger
    : ILogger
{

    public ILogHandler logHandler { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
    public bool logEnabled { get { throw new NotImplementedException(); }  set { throw new NotImplementedException(); } }
    public LogType filterLogType { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

    public bool IsLogTypeAllowed(LogType logType)
    {
        throw new NotImplementedException();
    }

    public void Log(LogType logType, object message)
    {
        throw new NotImplementedException();
    }

    public void Log(LogType logType, object message, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }

    public void Log(LogType logType, string tag, object message)
    {
        throw new NotImplementedException();
    }

    public void Log(LogType logType, string tag, object message, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }

    public void Log(object message)
    {
        throw new NotImplementedException();
    }

    public void Log(string tag, object message)
    {
        throw new NotImplementedException();
    }

    public void Log(string tag, object message, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }

    public void LogError(string tag, object message)
    {
        throw new NotImplementedException();
    }

    public void LogError(string tag, object message, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }

    public void LogException(Exception exception)
    {
        throw new NotImplementedException();
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }

    public void LogFormat(LogType logType, string format, params object[] args)
    {
        throw new NotImplementedException();
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        throw new NotImplementedException();
    }

    public void LogWarning(string tag, object message)
    {
        throw new NotImplementedException();
    }

    public void LogWarning(string tag, object message, UnityEngine.Object context)
    {
        throw new NotImplementedException();
    }
}
