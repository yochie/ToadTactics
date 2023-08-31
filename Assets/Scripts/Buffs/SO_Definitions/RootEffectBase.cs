using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class RootEffectBase : IAppliablBuff
{
    public abstract bool NeedsToBeReAppliedEachTurn { get; }
    public abstract string stringID { get; set; }
    public abstract string UIName { get; set; }
    public abstract bool IsPositive { get; set; }
    public abstract DurationType DurationType { get; set; }
    public abstract int TurnDuration { get; set; }
    public abstract Image Icon { get; set; }
    public abstract string tooltipDescription { get; set; }

    //public bool NeedsToBeReAppliedEachTurn => true;
    //public bool IsPositive => false;

    ////set in subclass definition
    //public abstract string BuffTypeID { get; }
    //public abstract string IconName { get; } 
    //public abstract string UIName { get; }

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication = false)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs) 
        { 
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(false);
        }
        
        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(true);
        }
    }
}
