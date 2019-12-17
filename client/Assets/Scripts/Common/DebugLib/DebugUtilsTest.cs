using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DebugUtilsTest : MonoBehaviour
{
    private Exception e = new Exception("test log exception");

    // Start is called before the first frame update
    void Start()
    {
        UFK.DebugUtils.LogFilePath = "";
        UFK.DebugUtils.logEnabled = true;
        UFK.DebugUtils.Startup();
        Debug.LogFormat("{0}",UFK.DebugUtils.LogFilePath);

        this.Print();
        new Thread(this.Print).Start();
        new Thread(this.Print).Start();
    }

    private void Print()
    {
        for (int i = 0, iMax = 999; i < iMax; ++i)
        {
            Debug.Log(i);
            Debug.LogWarning(i);
            Debug.LogError(i);
            Debug.LogException(e);
        }
    }
    
    private void OnApplicationQuit()
    {
        UFK.DebugUtils.Release();
    }
}
