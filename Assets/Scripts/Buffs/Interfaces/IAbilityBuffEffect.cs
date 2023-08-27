using System.Collections.Generic;

public interface IAbilityBuffEffect : IBuff
{
    public int ApplyingCharacterID { get; set; }
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
