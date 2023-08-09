public interface IAbilityAction : IAction
{
    public CharacterAbilityStats AbilityStats { get; set; }

    public static bool ValidateCooldowns(IAbilityAction action)
    {
        if (!action.ActorCharacter.AbilityUsesPerRoundExpended(action.AbilityStats.stringID) &&
            !action.ActorCharacter.AbilityOnCooldown(action.AbilityStats.stringID))
            return true;
        else
            return false;
        
    }
}
