using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageModifier : IStatModifier
{
    public int DamageOffset { get; set; }
}
