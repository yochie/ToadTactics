using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ActionGameEventSO : ScriptableObject
{
    private readonly List<ActionGameEventSOListener> eventListeners = new();

    public void Raise(IAction actionArg)
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(actionArg);
    }

    public void RegisterListener(ActionGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(ActionGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}