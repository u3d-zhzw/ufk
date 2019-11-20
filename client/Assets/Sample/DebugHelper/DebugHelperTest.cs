using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelperTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.Log("t1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
