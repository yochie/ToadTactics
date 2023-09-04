using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/StringIntGameEventSO", order = 3)]
public class StringIntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<StringIntGameEventSOListener> eventListeners = new();

    public void Raise(string stringArg1, int intArg2)
    {
        string message = string.Format("{0} raised", this.name);
        Debug.Log(message);
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(stringArg1, intArg2);
    }

    public void RegisterListener(StringIntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(StringIntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }

    public Type GetListenerType()
    {
        return typeof(StringIntGameEventSOListener);
    }
}