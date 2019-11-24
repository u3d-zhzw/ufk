using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelperTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UFKCore.DebugHelper.Startup();
        UFKCore.DebugHelper.logEnabled = true;
        Debug.Log("t1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        UFKCore.DebugHelper.Release();
    }
}
