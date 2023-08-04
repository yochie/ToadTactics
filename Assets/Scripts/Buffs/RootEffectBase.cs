using UnityEngine;

public abstract class RootEffectBase : IBuffEffect, ITimedEffect
{
    public bool NeedsToBeReAppliedEachTurn => true;
    public bool IsPositive => false;

    //set in subclass definition
    public abstract string StringID { get; }
    public abstract string IconName { get; } 
    public abstract string UIName { get; }

    //set at runtime
    public int AffectedCharacterID { get; set; }
    public int TurnDurationRemaining { get; set; }

    public bool ApplyEffect(bool isReapplication = false)
    {
        PlayerCharacter appliedTo = GameController.Singleton.PlayerCharacters[this.AffectedCharacterID];
        appliedTo.SetCanMove(false);
        
        return true;
    }

    public void UnApply()
    {
        PlayerCharacter appliedTo = GameController.Singleton.PlayerCharacters[this.AffectedCharacterID];
        appliedTo.SetCanMove(true);
    }
}
