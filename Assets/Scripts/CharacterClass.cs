using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct CharacterClass
{
    public readonly string className;
    public readonly string classDescription;
    public readonly CharacterStats charStats;
    public readonly CharacterAbility charAbility;

    public CharacterClass(string name, string description, CharacterStats stats, CharacterAbility ability)
    {
        this.className = name;
        this.classDescription = description;
        this.charStats = stats;
        this.charAbility = ability;
    }
}
