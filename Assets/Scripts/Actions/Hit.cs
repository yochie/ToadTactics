public struct Hit
{
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly bool penetratesArmor;

    public Hit(int damage, DamageType damageType, bool penetratesArmor = false)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.penetratesArmor = penetratesArmor;
    }
}