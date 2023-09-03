using UnityEngine;
using UnityEngine.Events;

public class StringIntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public StringIntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public StringIntEvent Response = new();

    private void OnEnable()
    {
        if (this.Event == null)
            return;
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (this.Event == null)
            return;
        Event.UnregisterListener(this);
    }

    public void RegisterManually()
    {
        if (this.Event == null)
            return;
        this.Event.RegisterListener(this);
    }

    public void OnEventRaised(string stringArg1, int intArg2)
    {
        Response.Invoke(stringArg1, intArg2);
    }
}