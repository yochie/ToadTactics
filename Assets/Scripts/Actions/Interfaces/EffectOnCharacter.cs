using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct EffectOnCharacter
{
    public readonly int classID;
    public readonly HexCoordinates position;
    public readonly int minDamage;
    public readonly int maxDamage;
    //public readonly Vector3 knockBackDirection;

    public EffectOnCharacter(int classID, HexCoordinates position, int minDamage, int maxDamage)
    {
        this.classID = classID;
        this.position = position;
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
    }
}
