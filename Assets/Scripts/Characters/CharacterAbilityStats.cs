using System;
using System.Collections.Generic;

public readonly struct CharacterAbilityStats
{
    public readonly string stringID;
    public readonly string interfaceName;
    public readonly string description;
    public readonly int turnDuration;
    public readonly int damage;
    public readonly int damageIterations;
    public readonly DamageType damageType;
    public readonly int range;
    public readonly int aoe;
    public readonly bool requiresLOS;
    public readonly Type actionType;
    public readonly List<TargetType> allowedAbilityTargets;

    public CharacterAbilityStats(string stringID,
                            string interfaceName,
                            string description,
                            int damage,
                            int range,
                            int aoe,
                            int turnDuration,
                            bool requiresLOS = true,
                            int damageIterations = 1,
                            DamageType damageType = DamageType.physical,
                            Type actionType = null,
                            List<TargetType> allowedAbilityTargets = null)
    {
        this.stringID = stringID;
        this.interfaceName = interfaceName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.aoe = aoe;
        this.turnDuration = turnDuration;        
        this.requiresLOS = requiresLOS;
        this.damageIterations = damageIterations;
        this.damageType = damageType;
        this.actionType = actionType;

        if (allowedAbilityTargets == null)
        {
            this.allowedAbilityTargets = new List<TargetType> { TargetType.ennemy_chars};
        }
        else
        {
            this.allowedAbilityTargets = new List<TargetType>(allowedAbilityTargets);
        }
    }
}
