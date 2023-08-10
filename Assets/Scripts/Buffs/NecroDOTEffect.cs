using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NecroDOTEffect : IAbilityBuffEffect, IPermanentEffect
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

    #region IBuffEffect functions
    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication)
    {
        if (!isReapplication)
            return false;

        Debug.Log("Reapplying Necro DOT effect.");

        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.TakeDamage(this.AppliedByAbility.damage, this.AppliedByAbility.damageType);
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
