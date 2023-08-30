using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RuntimeBuff
{
    public IBuffDataSO BuffData { get; set; }
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    private readonly Dictionary<Type, IRuntimeBuffComponent> buffComponents = new();

    public IRuntimeBuffComponent GetComponent(Type type)
    {
        if (this.buffComponents.ContainsKey(type))
            return this.buffComponents[type];
        else
            return null;
    }

    public void AddComponent(IRuntimeBuffComponent component)
    {
        this.buffComponents[component.GetType()] = component;
    }
}
