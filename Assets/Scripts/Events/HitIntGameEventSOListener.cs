using UnityEngine;
using UnityEngine.Events;

public class HitIntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public HitIntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public HitIntEvent Response = new();

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

    public void OnEventRaised(Hit hit, int intArg)
    {
        Response.Invoke(hit, intArg);
    }
}