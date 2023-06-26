using System.Collections.Generic;

public readonly struct CharacterAbility
{
    public readonly string name;
    public readonly string description;
    public readonly int turnDuration;
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly int range;
    public readonly int aoe;

    public delegate void Use(PlayerCharacter user, List<Hex> targets);
    public readonly Use use;

    public CharacterAbility(string name, string description, int damage, int range, int aoe, int turnDuration, Use use, DamageType damageType = DamageType.normal)
    {
        this.name = name;
        this.description = description;
        this.damage = damage;
        this.damageType = damageType;
        this.range = range;
        this.aoe = aoe;
        this.turnDuration = turnDuration;
        this.use = use;
    }
}
