public interface IAbilityBuffEffect : IBuffEffect
{
    public int AppliedByCharacterID { get; set; }
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
