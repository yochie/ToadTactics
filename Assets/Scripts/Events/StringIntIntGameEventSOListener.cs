using UnityEngine;
using UnityEngine.Events;

public class StringIntIntGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public StringIntIntGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public StringIntIntEvent Response = new();

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(string stringArg1, int intArg2, int intArg3)
    {
        Response.Invoke(stringArg1, intArg2, intArg3);
    }
}