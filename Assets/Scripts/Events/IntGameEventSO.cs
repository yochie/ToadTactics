using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class IntGameEventSO : ScriptableObject
{
    private readonly List<IntGameEventSOListener> eventListeners = new();

    public void Raise(int intArg)
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(intArg);
    }

    public void RegisterListener(IntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(IntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}