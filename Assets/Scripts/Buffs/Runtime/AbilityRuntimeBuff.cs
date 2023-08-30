using System.Collections.Generic;

public class AbilityRuntimeBuff : IRuntimeBuffComponent
{
    public int ApplyingCharacterID { get; set; }
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
