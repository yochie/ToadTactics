public struct CharacterStats
{
    public int MaxHealth { get; set; }
    public int Armor { get; set; }
    public int Damage { get; set; }
    public int DamageIterations { get; set; }
    public int Speed { get; set; }
    public int Initiative { get; set; }
    public int Range { get; set; }

    public CharacterStats(int maxHealth, int armor, int damage, int speed, int initiative, int range = 1, int damageIterations = 1)
    {
        this.MaxHealth = maxHealth;
        this.Armor = armor;
        this.Damage = damage;
        this.Speed = speed;
        this.Initiative = initiative;
        this.Range = range;
        this.DamageIterations = damageIterations;
    }

}