using System;
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

    public static EffectOnCharacter None()
    {
        return new EffectOnCharacter(-1, HexCoordinates.None(), 0, 0);
    }

    internal EffectOnCharacter Add(EffectOnCharacter effectToAdd)
    {
        if(this.classID != effectToAdd.classID)
        {
            throw new Exception("Adding action effect previews from different characters is not supported");
        }
        HexCoordinates newPosition = effectToAdd.position;
        int newMinDamage = this.minDamage + effectToAdd.minDamage;
        int newMaxDamage = this.maxDamage + effectToAdd.maxDamage;
        return new EffectOnCharacter(this.classID, newPosition, newMinDamage, newMaxDamage);
    }
}
