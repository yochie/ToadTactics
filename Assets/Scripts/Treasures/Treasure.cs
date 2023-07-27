using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public readonly struct Treasure
{
    public readonly uint treasureID;
    public readonly string name;
    public readonly TreasureQuality quality;
    public readonly CharacterStats statBonus;
    public readonly bool isActivated;
    public readonly TreasureAction ability;

    public Treasure(uint treasureID, string name, TreasureQuality quality, CharacterStats statBonus, bool isActivated, TreasureAction ability)
    {
        this.treasureID = treasureID;
        this.name = name;
        this.quality = quality;
        this.statBonus = statBonus;
        this.isActivated = isActivated;
        this.ability = ability;
    }
}
