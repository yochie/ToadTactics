using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BarbarianKingDamageEffect : IAbilityBuffEffect, IKingDamageModifier, IPassiveEffect
{
    #region IBuffEffect
    public string BuffTypeID => "BarbarianKingDamageEffect";
    public string UIName => "Kingslayer";
    public string IconName => "crown";
    public bool IsPositive => true;
    public bool NeedsToBeReAppliedEachTurn => false;
    // set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    #endregion

    #region  IKingDamageModifier
    private const int KING_DAMAGE_OFFSET = 20;

    public int KingDamageOffset { get => KING_DAMAGE_OFFSET; set => throw new NotSupportedException(); }

    #endregion

    #region IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }
    #endregion

    #region IBuffEffect functions
    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication)
    {
        foreach(int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.ApplyStatModification(affectedCharacter);
        }
        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.RemoveStatModification(affectedCharacter);
        }
    }
    #endregion

    #region IStatModifier functions
    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        throw new NotSupportedException();
    }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentKingDamage = playerCharacter.CurrentStats.kingDamage;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, kingDamage: currentKingDamage + this.KingDamageOffset));
        Debug.Log("Applying king dmg buff.");
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentKingDamage = playerCharacter.CurrentStats.kingDamage;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, kingDamage: currentKingDamage - this.KingDamageOffset));
    }
    #endregion
}
