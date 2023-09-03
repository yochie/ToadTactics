using UnityEngine;
using UnityEngine.Events;

public class GameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public GameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public UnityEvent Response;

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

    public void OnEventRaised()
    {
        Response.Invoke();
    }
}