using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Using composite design pattern to aggregate different loggers and dispatch logging to all of them
//not quite actually composite since this is Networked, aggregated loggers are local
public class MasterLogger : NetworkBehaviour, INetworkedLogger
{

    public static MasterLogger Singleton { get; private set; }

    private List<ILogger> loggers = new();

    private void Awake()
    {
        Debug.Log("Awaking MasterLogger");
        if (MasterLogger.Singleton != null)
        {
            Debug.Log("Destroying duplicated MasterLogger");
            Destroy(this.gameObject);
            return;
        }
        MasterLogger.Singleton = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying MasterLogger");
    }

    public void AddLogger(ILogger loggerToAdd)
    {
        this.loggers.Add(loggerToAdd);
    }

    public void RemoveLogger(ILogger loggerToRemove)
    {
        this.loggers.Remove(loggerToRemove);
    }

    public void LogMessage(string message)
    {
        foreach(ILogger logger in this.loggers) 
        {
            if (logger == null)
                throw new System.Exception("Logger hasn't been properly removed for Master logger list.");
            logger.LogMessage(message);
        }
    }

    [ClientRpc]
    public void RpcLogMessage(string message)
    {
        this.LogMessage(message);
    }
}
