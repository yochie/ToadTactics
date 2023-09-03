using UnityEngine;
using UnityEngine.Events;

public class IntIntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public IntIntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public IntIntEvent Response = new();

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

    public void OnEventRaised(int intArg1, int intArg2)
    {
        Response.Invoke(intArg1, intArg2);
    }
}