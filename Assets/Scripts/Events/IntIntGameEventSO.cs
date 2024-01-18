using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/IntIntGameEventSO", order = 3)]
public class IntIntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<IntIntGameEventSOListener> eventListeners = new();

    public void Raise(int intArg1, int intArg2)
    {
        //string message = string.Format("{0} raised", this.name);
        //Debug.Log(message);
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

    public Type GetListenerType()
    {
        return typeof(IntIntGameEventSOListener);
    }
}