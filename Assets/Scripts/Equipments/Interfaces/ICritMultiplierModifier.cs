using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICritMultiplierModifier : IStatModifier
{
    public float CritMultiplierOffset { get; set; }
}
