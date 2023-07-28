using UnityEngine;
using UnityEngine.Events;

public class IntIntIntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public IntIntIntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public IntIntIntEvent Response = new();

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(int intArg1, int intArg2, int intArg3)
    {
        Response.Invoke(intArg1, intArg2, intArg3);
    }
}