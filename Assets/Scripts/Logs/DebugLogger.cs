using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugLogger : MonoBehaviour, ILogger
{
    //only used to prevent duplicate
    private static DebugLogger Singleton { get; set; }

    private void Awake()
    {
        Debug.Log("Awaking DebugLogger");
        if (DebugLogger.Singleton != null)
        {
            Debug.Log("Destroying duplicated DebugLogger");
            Destroy(this.gameObject);
            return;
        }
        DebugLogger.Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        //register self after Awake since that is where masterLogger singleton is set
        //no need for unregistering, that 
        MasterLogger.Singleton.AddLogger(this);
    }

    public void LogMessage(string message)
    {
        Debug.Log(message);
    }

}
