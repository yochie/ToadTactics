using System;
using System.Collections.Generic;

public readonly struct CharacterAbilityStats
{
    public readonly string stringID;
    public readonly string interfaceName;
    public readonly string description;
    public readonly int buffTurnDuration;
    public readonly int damage;
    public readonly int damageIterations;
    public readonly DamageType damageType;
    public readonly int range;
    public readonly int aoe;
    public readonly bool requiresLOS;
    public readonly List<TargetType> allowedAbilityTargets;
    public readonly bool canCrit;
    //-1 = infinite
    public readonly int usesPerRound;    
    public readonly int cooldownDuration;

    //Todo: remove once all characters has abilities defined
    public CharacterAbilityStats(bool fake)
    {
        this.stringID = "fake";
        this.interfaceName = "";
        this.description = "";
        this.damage = 0;
        this.range = 0;
        this.aoe = 0;
        this.buffTurnDuration = 0;
        this.requiresLOS = false; ;
        this.damageIterations = 0; ;
        this.damageType = DamageType.none;
        this.allowedAbilityTargets = new();
        this.canCrit = false;
        this.usesPerRound = -1;
        this.cooldownDuration = 0;

    }

    public CharacterAbilityStats(string stringID,
                            string interfaceName,
                            string description,
                            int damage = 0,
                            int range = 0,
                            int aoe = 0,
                            int turnDuration = 0,
                            bool requiresLOS = true,
                            int damageIterations = 1,
                            DamageType damageType = DamageType.physical,
                            List<TargetType> allowedAbilityTargets = null,
                            bool canCrit = false,
                            int  usesPerRound = -1,
                            int cooldownDuration = 0)
    {
        this.stringID = stringID;
        this.interfaceName = interfaceName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.aoe = aoe;
        this.buffTurnDuration = turnDuration;        
        this.requiresLOS = requiresLOS;
        this.damageIterations = damageIterations;
        this.damageType = damageType;

        if (allowedAbilityTargets == null)
        {
            this.allowedAbilityTargets = new List<TargetType> { TargetType.ennemy_chars};
        }
        else
        {
            this.allowedAbilityTargets = new List<TargetType>(allowedAbilityTargets);
        }

        this.canCrit = canCrit;
        this.usesPerRound = usesPerRound;
        this.cooldownDuration = cooldownDuration;
    }
}
