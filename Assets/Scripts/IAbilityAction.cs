public interface IAbilityAction : IAction
{
    public CharacterAbilityStats abilityStats { get; set; }

    public PlayerCharacter user { get; set; }
}
