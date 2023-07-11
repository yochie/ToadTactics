using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class IntIntGameEventSO : ScriptableObject
{
    private readonly List<IntIntGameEventSOListener> eventListeners = new();

    public void Raise(int intArg1, int intArg2)
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(intArg1, intArg2);
    }

    public void RegisterListener(IntIntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(IntIntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}