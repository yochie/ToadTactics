using UnityEngine;

public interface IAbilityAction : IAction, ITargetedAction, IAreaTargeter
{
    public CharacterAbilityStats AbilityStats { get; set; }

    public static bool ValidateCooldowns(IAbilityAction action)
    {
        if (!action.ActorCharacter.AbilityUsesPerRoundExpended(action.AbilityStats.stringID) &&
            !action.ActorCharacter.AbilityOnCooldown(action.AbilityStats.stringID))
            return true;
        else
        { 
            Debug.Log("Cooldown validation failed");
            return false;
        }        
    }
}
