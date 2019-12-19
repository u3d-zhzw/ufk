using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWriter
{
    private StreamWriter writer = null;
    private string logFile = null;

    public long Length { get; set; }

    public DebugWriter(string logFile)
    {
        string dir = Path.GetDirectoryName(logFile);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        this.Length = 0;    
        this.writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.writer != null)
            {
                this.Length = 0;    
                this.writer.Close();
                this.writer.Dispose();
            }
            this.writer = null;

            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
    }

    private static string ToLogTypeString(LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                return "[L]";
            case LogType.Warning:
                return "[W]";
            case LogType.Error:
                return "[E]";
            case LogType.Exception:
                return "[X]";
            case LogType.Assert:
                return "[A]";
                // default:
                // return "UNKNOW";
        }
        return "UNKNOW";
    }

    public void Write(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        // todo: 
        // 1. 字符池尝试使用缓存池

        if (this.writer != null)
        {
            this.writer.WriteLine(value);

            // this.writer.WriteLine("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss:fff"), value);
        }

        this.Length += value.Length * 2;
    }

    public void Write(string condition, string stackTrace, LogType type)
    {
        string content = stackTrace;
        if (type != LogType.Error)
        {
            string[] s = stackTrace.Split('\n');
            if (s.Length >= 2)
            {
                content = s[1];
            }
        }
        
        Write(string.Format("[{0:D2}:{1:D2}:{2:D2}:{3:D3}] {4}\t{5}\n{6}\n",
                   DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond
                   , ToLogTypeString(type), condition, content));
    }

    public void Flush()
    {
        if (this.writer != null && this.Length > 0)
        {
            this.writer.Flush();
            this.Length = 0;
        }
    }
}
