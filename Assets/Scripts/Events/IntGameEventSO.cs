using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/IntGameEventSO", order = 2)]
public class IntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<IntGameEventSOListener> eventListeners = new();

    public void Raise(int intArg)
    {
        //string message = string.Format("{0} raised", this.name);
        //Debug.Log(message);
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

    public Type GetListenerType()
    {
        return typeof(IntGameEventSOListener);
    }
}