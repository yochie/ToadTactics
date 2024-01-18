using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameLogger : MonoBehaviour, ILogger
{
    [SerializeField]
    private GameObject logListOnScreen;

    [SerializeField]
    private LogEntry logEntryPrefab;

    [SerializeField]
    private Scrollbar scrollbar;

    [SerializeField]
    private ScrollRect scrollRect;

    private void Awake()
    {
        //Debug.Log("Awaking InGameLogger");
        if (MasterLogger.Singleton == null)
            throw new System.Exception("MasterLogger doesn't exist.");

        //Register to master Logger
        MasterLogger.Singleton.AddLogger(this);
    }

    private void OnDestroy()
    {
        MasterLogger.Singleton.RemoveLogger(this);
    }

    public void LogMessage(string message)
    {
        LogEntry logEntry = Instantiate(this.logEntryPrefab, this.logListOnScreen.transform);
        logEntry.SetText(message);
        StartCoroutine(this.ScrollToBottom());
    }


    private IEnumerator ScrollToBottom()
    {
        //Throwing everything ive got at this fking mess
        //currently working but its unclear if all this is required, would need more testing to cleanup
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.scrollRect.transform);
        Canvas.ForceUpdateCanvases();
        this.scrollbar.value = 0;
        this.scrollRect.verticalNormalizedPosition = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.scrollRect.transform);
        Canvas.ForceUpdateCanvases();
    }
}
