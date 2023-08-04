using UnityEngine;

public abstract class StunEffectBase : IBuffEffect, ITimedEffect
{
    public bool NeedsToBeReAppliedEachTurn => false;
    public bool IsPositive => false;

    //set in subclass definition
    public abstract string StringID { get;}
    public abstract string IconName { get;}
    public abstract string UIName { get; }

    //set at runtime
    public int AffectedCharacterID { get; set; }
    public int TurnDurationRemaining { get; set; }

    public bool ApplyEffect(bool isReapplication = false)
    {
        PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharacters[this.AffectedCharacterID];

        affectedCharacter.SetCanTakeTurns(false);
        return true;
    }

    public void UnApply()
    {
        PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharacters[this.AffectedCharacterID];
        affectedCharacter.SetCanTakeTurns(true);
    }
}