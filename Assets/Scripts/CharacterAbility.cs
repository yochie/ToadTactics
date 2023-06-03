public struct CharacterAbility
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int TurnDuration { get; private set; }
    public int Damage { get; private set; }
    public int Range { get; private set; }

    public delegate void Use(PlayerCharacter pc, Hex target);
    public Use use;

    public CharacterAbility(string name, string description, int damage, int range, int turnDuration, Use use )
    {
        this.Name = name;
        this.Description = description;
        this.Damage = damage;
        this.Range = range;
        this.TurnDuration = turnDuration;
        this.use = use;
    }
}
