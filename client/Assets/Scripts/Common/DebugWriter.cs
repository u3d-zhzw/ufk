using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWriter
{
    private StreamWriter writer = null;
    private string logFile = null;

    public DebugWriter(string logFile)
    {
        this.writer = new StreamWriter(logFile, true, System.Text.Encoding.UTF8);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.writer != null)
            {
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
        // todo: 
        // 1. 字符池尝试使用缓存池
        // 2. 分类Tag
        // 3. 限制每帧输出过多的Log

        this.writer.WriteLine("from DebugWriter" +  value);
        // todo: 缓存value，达到一个buffer size再flush
        this.writer.Flush();
    }
}
