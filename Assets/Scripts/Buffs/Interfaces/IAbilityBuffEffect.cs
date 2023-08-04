public interface IAbilityBuffEffect : IBuffEffect
{
    public int ApplyingCharacterID { get; set; }
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
