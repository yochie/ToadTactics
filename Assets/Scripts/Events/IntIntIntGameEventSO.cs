using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/IntIntIntGameEventSO", order = 3)]
public class IntIntIntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<IntIntIntGameEventSOListener> eventListeners = new();

    public void Raise(int intArg1, int intArg2, int intArg3)
    {
        //string message = string.Format("{0} raised", this.name);
        //Debug.Log(message);
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(intArg1, intArg2, intArg3);
    }

    public void RegisterListener(IntIntIntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(IntIntIntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }

    public Type GetListenerType()
    {
        return typeof(IntIntIntGameEventSOListener);
    }
}