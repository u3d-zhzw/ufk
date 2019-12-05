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

    public void Write(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        
        // todo: 
        // 1. 字符池尝试使用缓存池
        // 2. 分类Tag

        if (this.writer != null)
        {
            this.writer.WriteLine("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss:fff"), value);
        }

        this.Length += value.Length;
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
