using System;
using System.Collections.Generic;

class App
{
    /// <summary>
    /// 日志logger
    /// </summary>
    public Logger logger = new Logger();


    /// <summary>
    /// 服务端时间，单位毫秒
    /// </summary>
    private ulong _mstime;
    public ulong mstime
    {
        get
        {
            //TODO
            return this._mstime;
        }
    }


    public void Init()
    {
        // TODO: 
    }

    public void SetServerTime(ulong ms)
    {
        this._mstime = ms; 
    }

    public ulong GetServerTime()
    {
        return this._mstime;
    }
}
