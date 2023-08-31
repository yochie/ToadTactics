using System.Collections.Generic;

public class RuntimeBuffAbility : IRuntimeBuffComponent
{
    public int ApplyingCharacterID { get; set; }
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
