using System;
using System.Collections.Generic;
using UnityEngine;

struct AbilityPrintData
{
    //main table entry
    public readonly string name;
    public readonly string description;
    public readonly string currentCooldown;
    public readonly string currentRemainingUses;

    //tooltip
    public readonly string passiveOrActive;

    //main stats table
    public readonly Dictionary<string, string> abilityStatsDictionary;

    //buff stats table
    public readonly Dictionary<string, string> buffStatsDictionary;
    public readonly string buffOrDebuff;

    public AbilityPrintData (string name,
                            string description,
                            string currentCooldown = "",
                            string currentRemainingUses = "",
                            string passiveOrActive = "",
                            string buffOrDebuff = "",
                            Dictionary<string, string> statsDictionary  = null,
                            Dictionary<string, string> buffsDictionary = null
                            )
    {
        this.name = name;
        this.description = description;
        this.currentCooldown = currentCooldown;
        this.currentRemainingUses = currentRemainingUses;
        this.passiveOrActive = passiveOrActive;
        this.buffOrDebuff = buffOrDebuff;
        this.abilityStatsDictionary = statsDictionary;
        this.buffStatsDictionary = buffsDictionary;

    }
}