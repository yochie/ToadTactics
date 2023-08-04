using UnityEngine;

public abstract class StunEffectBase : ScriptableObject, IBuffEffect, ITimedEffect
{
    //Set in base SO
    public abstract bool NeedsToBeReAppliedEachTurn { get; set; }    
    public abstract bool IsPositive { get; set; }

    //set in subclasses
    public abstract string StringID { get; set; }
    public abstract Sprite Icon { get; set; }
    public abstract string UIName { get; set; }

    //set at runtime
    public abstract int AppliedToCharacterID { get; set; }
    public abstract int TurnDurationRemaining { get; set; }

    public bool ApplyEffect(bool isReapplication = false)
    {
        if(isReapplication)
            GameController.Singleton.CmdNextTurn();
        return true;
    }

    public void UnApply()
    {
        //nothing to do...
    }
}