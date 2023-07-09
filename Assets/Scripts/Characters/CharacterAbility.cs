using System;
using System.Collections.Generic;

public readonly struct CharacterAbility

{
    public readonly string name;
    public readonly string description;
    public readonly int turnDuration;
    public readonly int damage;
    public readonly int damageIterations;
    public readonly DamageType damageType;
    public readonly int range;
    public readonly int aoe;
    public readonly bool requiresLOS;
    public readonly Type abilityActionType;

    public CharacterAbility(string name,
                            string description,
                            int damage,
                            int range,
                            int aoe,
                            int turnDuration,
                            Type abilityActionType,
                            bool requiresLOS = true,
                            int damageIterations = 1,
                            DamageType damageType = DamageType.normal)
    {
        this.name = name;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.aoe = aoe;
        this.turnDuration = turnDuration;
        this.abilityActionType = abilityActionType;
        this.requiresLOS = requiresLOS;
        this.damageIterations = damageIterations;
        this.damageType = damageType;
    }
}
