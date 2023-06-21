using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class ITreasure
{
    public TreasureQuality quality;
    public string name;
    public bool isActivated;
    public CharacterStats statBonus;
    public abstract void Use();
}
