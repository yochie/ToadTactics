public struct CharacterAbility
{
    private string name;
    private string description;
    private int turnDuration;
    private int damage;
    private int range;
    public delegate void Use(PlayerCharacter pc, Hex target);
    public Use use;

    public CharacterAbility(string name, string description, int damage, int range, int turnDuration, Use use )
    {
        this.name = name;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.turnDuration = turnDuration;
        this.use = use;
    }
}
