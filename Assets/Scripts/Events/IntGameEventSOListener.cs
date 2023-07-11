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
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(int intArg)
    {
        Response.Invoke(intArg);
    }
}