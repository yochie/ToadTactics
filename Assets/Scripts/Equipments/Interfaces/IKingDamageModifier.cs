using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKingDamageModifier : IStatModifier
{
    public int KingDamageOffset { get; set; }
}
