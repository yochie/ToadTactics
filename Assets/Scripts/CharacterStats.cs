public struct CharacterStats
{
    public readonly int maxHealth;
    public readonly int armor;
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly int damageIterations;
    public readonly int moveSpeed;
    public readonly int initiative;
    public readonly int range;

    public CharacterStats(int maxHealth, int armor, int damage, int moveSpeed, int initiative, int range = 1, int damageIterations = 1, DamageType damageType = DamageType.normal)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.damage = damage;
        this.damageType = damageType;
        this.damageIterations = damageIterations;
        this.moveSpeed = moveSpeed;
        this.initiative = initiative;
        this.range = range;
    }

}