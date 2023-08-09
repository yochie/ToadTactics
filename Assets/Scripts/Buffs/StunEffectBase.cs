using System.Collections.Generic;
using UnityEngine;

public abstract class StunEffectBase : IBuffEffect, ITimedEffect
{
    public bool NeedsToBeReAppliedEachTurn => false;
    public bool IsPositive => false;

    //set in subclass definition
    public abstract string BuffTypeID { get;}
    public abstract string IconName { get;}
    public abstract string UIName { get; }

    //set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }
    public int TurnDurationRemaining { get; set; }

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication = false)
    {
        foreach(int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.SetCanTakeTurns(false);
        }

        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.SetCanTakeTurns(true);
        }
    }
}