using System.Collections.Generic;

public readonly struct CharacterAbilityStats

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

    public CharacterAbilityStats(string name, string description, int damage, int range, int aoe, int turnDuration, bool requiresLOS = true,int damageIterations = 1, DamageType damageType = DamageType.normal)
    {
        this.name = name;
        this.description = description;
        this.damage = damage;
        this.damageIterations = damageIterations;
        this.damageType = damageType;
        this.range = range;
        this.aoe = aoe;
        this.turnDuration = turnDuration;
        this.requiresLOS = requiresLOS;
    }
}
