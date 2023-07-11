using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEventSO : ScriptableObject
{
    private readonly List<GameEventSOListener> eventListeners = new List<GameEventSOListener>();

    public void Raise()
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
            eventListeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventSOListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(GameEventSOListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}