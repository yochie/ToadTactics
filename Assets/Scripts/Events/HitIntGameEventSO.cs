using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/HitIntGameEventSO", order = 2)]
public class HitIntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<HitIntGameEventSOListener> eventListeners = new();

    public void Raise(Hit hit, int intArg)
    {
        //string message = string.Format("{0} raised", this.name);
        //Debug.Log(message);
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(hit, intArg);
    }

    public void RegisterListener(HitIntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(HitIntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }

    public Type GetListenerType()
    {
        return typeof(HitIntGameEventSOListener);
    }
}