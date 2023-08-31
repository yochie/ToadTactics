using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RuntimeBuff
{
    public IBuffDataSO Data { get; set; }
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    private readonly Dictionary<Type, IRuntimeBuffComponent> buffComponents = new();

    public T GetComponent<T>() where T : class, IRuntimeBuffComponent
    {
        if (this.buffComponents.ContainsKey(typeof(T)))
            return (T) this.buffComponents[typeof(T)];
        else
            return null;
    }

    public void AddComponent(IRuntimeBuffComponent component)
    {
        this.buffComponents[component.GetType()] = component;
    }
}
