using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NecroDOTEffect : IAbilityBuffEffect, IPermanentEffect, IDisplayedBuff, IAppliedBuff
{
    #region IBuffEffect
    public string BuffTypeID => "NecroDOTEffect";
    public string UIName => "Rotting Corpse";
    public string IconName => "skull";
    public bool IsPositive => false;
    public bool NeedsToBeReAppliedEachTurn => true;

    // set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    #endregion

    #region IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }
    #endregion

    private const int DOT_DAMAGE = 10;
    private const DamageType DOT_DAMAGE_TYPE = DamageType.magic;

    #region IBuffEffect functions
    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication)
    {
        if (!isReapplication)
            return false;

        Debug.Log("Reapplying Necro DOT effect.");

        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.TakeDamage(DOT_DAMAGE, DOT_DAMAGE_TYPE);
        }
        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        //nothing to do
        return;
    }
    #endregion
}
