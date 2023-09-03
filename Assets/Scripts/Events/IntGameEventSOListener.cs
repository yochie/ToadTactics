using UnityEngine;
using UnityEngine.Events;

public class IntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public IntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public IntEvent Response = new();

    private void OnEnable()
    {
        if (this.Event == null)
            return;
        this.Event.RegisterListener(this);
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

    public void OnEventRaised(int intArg)
    {
        Response.Invoke(intArg);
    }
}