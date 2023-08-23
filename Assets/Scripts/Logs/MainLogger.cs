using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainLogger : NetworkBehaviour, INetworkedLogger
{
    public static MainLogger Singleton { get; private set; }

    private void Awake()
    {
        Debug.Log("Awaking MainLogger");
        if (MainLogger.Singleton != null)
        {
            Debug.Log("Destroying old mainlogger");
            Destroy(MainLogger.Singleton.gameObject);
            return;
        }            
        MainLogger.Singleton = this;
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

    [ClientRpc]
    public void RpcLogMessage(string message)
    {
        this.LogMessage(message);
    }
}
