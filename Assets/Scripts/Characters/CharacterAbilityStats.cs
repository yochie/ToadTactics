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
    //-1 = use character stat
    public readonly float critChance;
    //-1 = use character stat
    public readonly float critMultiplier;
    //-1 = infinite
    public readonly int usesPerRound;
    public readonly int cooldownDuration;
    public readonly bool isPassive;
    public readonly bool penetratingDamage;
    public readonly bool piercesLOS;

    public readonly bool cappedPerRound;
    public readonly bool cappedByCooldown;

    public CharacterAbilityStats(string stringID,
                            string interfaceName,
                            string description,
                            int damage = -1,
                            int range = -1,
                            int aoe = -1,
                            int buffTurnDuration = -1,
                            bool requiresLOS = true,
                            int damageIterations = -1,
                            DamageType damageType = DamageType.none,
                            List<TargetType> allowedAbilityTargets = null,
                            bool canCrit = false,
                            float critChance = -1f,
                            float critMultiplier = -1f,
                            int usesPerRound = -1,
                            int cooldownDuration = -1,
                            bool isPassive = false,
                            bool penetratingDamage = false,
                            bool piercesLOS = false,
                            bool cappedPerRound = false,
                            bool cappedByCooldown = false
                            )
    {
        this.stringID = stringID;
        this.interfaceName = interfaceName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.aoe = aoe;
        this.buffTurnDuration = buffTurnDuration;        
        this.requiresLOS = requiresLOS;
        this.damageIterations = damageIterations;
        this.damageType = damageType;

        if (allowedAbilityTargets == null)
        {
            this.allowedAbilityTargets = new();
        }
        else
        {
            this.allowedAbilityTargets = new List<TargetType>(allowedAbilityTargets);
        }

        this.canCrit = canCrit;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
        this.usesPerRound = usesPerRound;
        this.cooldownDuration = cooldownDuration;
        this.isPassive = isPassive;
        this.penetratingDamage = penetratingDamage;
        this.piercesLOS = piercesLOS;
        this.cappedPerRound = cappedPerRound;
        this.cappedByCooldown = cappedByCooldown;
    }
}
