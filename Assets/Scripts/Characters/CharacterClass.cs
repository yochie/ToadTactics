using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CharacterClass
{
    public readonly int classID;
    public readonly string name;
    public readonly string description;
    public readonly CharacterStats stats;
    public readonly List<CharacterAbilityStats> abilities = new();
    public readonly string moveActionID;
    public readonly string attackActionID;
    public CharacterClass(int classID, string name, string description, CharacterStats stats, List<CharacterAbilityStats> abilities = null, string moveActionID = "DefaultMoveAction", string attackActionID = "DefaultAttackAction")
    {
        this.classID = classID;
        this.name = name;
        this.description = description;
        this.stats = stats;
        if (abilities == null)
            abilities = new();
        this.abilities = abilities;
        this.moveActionID = moveActionID;
        this.attackActionID = attackActionID;
    }
}
