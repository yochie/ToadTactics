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
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(int intArg1, int intArg2)
    {
        Response.Invoke(intArg1, intArg2);
    }
}