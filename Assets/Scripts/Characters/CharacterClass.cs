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
    public readonly List<CharacterAbility> abilities = new();

    public CharacterClass(int classID, string name, string description, CharacterStats stats, List<CharacterAbility> abilities = null)
    {
        this.classID = classID;
        this.name = name;
        this.description = description;
        this.stats = stats;
        this.abilities = abilities;
    }

    public Dictionary<string, string> GetPrintableAbilityDictionary()
    {
        Dictionary<string, string> toPrint = new();

        //return empty list if no abilities
        if (this.abilities == null)
            return toPrint;

        foreach (CharacterAbility ability in this.abilities)
        {
            toPrint.Add(ability.interfaceName, ability.description);
        }        

        return toPrint;
    }
}
