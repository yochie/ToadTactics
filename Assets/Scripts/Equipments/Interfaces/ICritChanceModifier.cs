using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICritChanceModifier : IStatModifier
{
    public float CritChanceOffset { get; set; }
}
