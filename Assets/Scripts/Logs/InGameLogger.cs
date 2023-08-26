using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class InGameLogger : NetworkBehaviour, ILogger
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
        this.UpdateLayout(this.transform);
        this.scrollbar.value = 0;
        this.scrollRect.verticalNormalizedPosition = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.scrollRect.transform);
        Canvas.ForceUpdateCanvases();
    }

    //https://forum.unity.com/threads/scroll-to-the-bottom-of-a-scrollrect-in-code.310919/
    private void UpdateLayout(Transform transform)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.scrollRect.transform);
        UpdateLayout_Internal(this.transform.parent);
    }

    private void UpdateLayout_Internal(Transform transform)
    {
        if (transform == null || transform.Equals(null))
        {
            return;
        }

        // Update children first
        for (int i = 0; i < transform.childCount; i++)
        {
            UpdateLayout_Internal(transform.GetChild(i));
        }

        // Update any components that might resize UI elements
        foreach (var layout in transform.GetComponents<LayoutGroup>())
        {
            layout.CalculateLayoutInputVertical();
            layout.CalculateLayoutInputHorizontal();
        }
        foreach (var fitter in transform.GetComponents<ContentSizeFitter>())
        {
            fitter.SetLayoutVertical();
            fitter.SetLayoutHorizontal();
        }
    }

}
