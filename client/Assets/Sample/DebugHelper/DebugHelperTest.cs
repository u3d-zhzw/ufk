using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DebugHelperTest : MonoBehaviour
{
    private Exception e = new Exception("test log exception");

    // Start is called before the first frame update
    void Start()
    {
        UFKCore.DebugHelper.Startup();
        UFKCore.DebugHelper.logEnabled = true;

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
        UFKCore.DebugHelper.Release();
    }
}
