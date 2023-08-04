using UnityEngine;

public abstract class RootEffectBase : IBuffEffect, ITimedEffect
{
    //Set in base SO
    [field: SerializeField]
    public bool NeedsToBeReAppliedEachTurn { get; set; }
    [field: SerializeField]
    public bool IsPositive { get; set; }

    //set in subclasses
    public abstract string StringID { get; set; }
    public abstract Sprite Icon { get; set; }
    public abstract string UIName { get; set; }


    //set at runtime
    public abstract int AppliedToCharacterID { get; set; }
    public int TurnDurationRemaining { get; set; }

    private int moveSpeedToRestore;

    public bool ApplyEffect(bool isReapplication = false)
    {
        PlayerCharacter appliedTo = GameController.Singleton.playerCharacters[this.AppliedToCharacterID];
        int currentMovespeed = appliedTo.currentStats.moveSpeed;

        //save or update previously saved value
        if (!isReapplication)
            this.moveSpeedToRestore = appliedTo.currentStats.moveSpeed;
        else if (currentMovespeed != 0)
            this.moveSpeedToRestore += currentMovespeed;


        appliedTo.currentStats = new CharacterStats(appliedTo.currentStats, moveSpeed : 0);
        
        return true;
    }

    public void UnApply()
    {
        PlayerCharacter appliedTo = GameController.Singleton.playerCharacters[this.AppliedToCharacterID];
        appliedTo.currentStats = new CharacterStats(appliedTo.currentStats, moveSpeed: this.moveSpeedToRestore);
    }
}
