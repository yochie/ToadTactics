public readonly struct Hit
{
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly bool penetratesArmor;
    public readonly HitSource hitSource;
    public readonly bool isCrit;


    public Hit(int damage, DamageType damageType, HitSource hitSource, bool isCrit, bool penetratesArmor = false)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.penetratesArmor = penetratesArmor;
        this.hitSource = hitSource;
        this.isCrit = isCrit;
    }
}

public enum HitSource
{
    CharacterAttack, Apple, FireHazard, Debuff, Buff
}