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
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(string stringArg1, int intArg2)
    {
        Response.Invoke(stringArg1, intArg2);
    }
}