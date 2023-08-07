using System.Collections.Generic;
using UnityEngine;

public abstract class RootEffectBase : IBuffEffect, ITimedEffect
{
    public bool NeedsToBeReAppliedEachTurn => true;
    public bool IsPositive => false;

    //set in subclass definition
    public abstract string BuffTypeID { get; }
    public abstract string IconName { get; } 
    public abstract string UIName { get; }

    //set at runtime
    public List<int> AffectedCharacterIDs { get; set; }
    public int TurnDurationRemaining { get; set; }
    public int UniqueID { get; set; }

    public bool ApplyEffect(bool isReapplication = false)
    {
        foreach (int affectedCharacterID in this.AffectedCharacterIDs) 
        { 
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(false);
        }
        
        return true;
    }

    public void UnApply()
    {
        foreach (int affectedCharacterID in this.AffectedCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(true);
        }
    }
}