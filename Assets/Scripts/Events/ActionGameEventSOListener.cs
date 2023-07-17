using UnityEngine;
using UnityEngine.Events;

public class ActionGameEventSOListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public ActionGameEventSO Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public ActionEvent Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(IAction actionArg)
    {
        Response.Invoke(actionArg);
    }
}