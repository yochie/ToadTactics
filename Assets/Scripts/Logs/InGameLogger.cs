using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InGameLogger : NetworkBehaviour, ILogger
{

    private void Awake()
    {
        Debug.Log("Awaking InGameLogger");
        if (MasterLogger.Singleton == null)
            throw new System.Exception("MasterLogger doesn't exist.");

        //Register to master Logger
        MasterLogger.Singleton.AddLogger(this);
    }

    private void OnDestroy()
    {
        MasterLogger.Singleton.RemoveLogger(this);
    }

    [SerializeField]
    private GameObject logListOnScreen;

    [SerializeField]
    private LogEntry logEntryPrefab;

    public void LogMessage(string message)
    {
        LogEntry logEntry = Instantiate(this.logEntryPrefab, this.logListOnScreen.transform);
        logEntry.SetText(message);
    }



}
