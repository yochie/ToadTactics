using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On", menuName = "Events/StringIntIntGameEventSO", order = 3)]
public class StringIntIntGameEventSO : ScriptableObject, IGameEventSO
{
    private readonly List<StringIntIntGameEventSOListener> eventListeners = new();

    public void Raise(string stringArg1, int intArg2, int intArg3)
    {
        //string message = string.Format("{0} raised", this.name);
        //Debug.Log(message);
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised(stringArg1, intArg2, intArg3);
    }

    public void RegisterListener(StringIntIntGameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(StringIntIntGameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }

    public Type GetListenerType()
    {
        return typeof(StringIntIntGameEventSOListener);
    }
}