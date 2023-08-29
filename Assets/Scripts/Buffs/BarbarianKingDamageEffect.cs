using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BarbarianKingDamageEffect : IAbilityBuffEffect, IPassiveEffect, IAttackEnhancer
{
    #region IBuff
    public string BuffTypeID => "BarbarianKingDamageEffect";
    public string UIName => "Kingslayer";
    // set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }
    #endregion

    private const int KING_DAMAGE_BONUS = 20;

    #region IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }

    public bool IsPositive => true;
    #endregion

    [Server]
    public IAttackAction EnhanceAttack(IAttackAction attackToEnhance)
    {

        if (attackToEnhance.TargetHex.HoldsACharacter() && attackToEnhance.TargetHex.GetHeldCharacterObject().IsKing)
        {
            attackToEnhance.Damage = attackToEnhance.Damage + KING_DAMAGE_BONUS;
        }
        return attackToEnhance;
    }

    public Dictionary<string, string> GetAbilityBuffPrintoutDictionnary()
    {
        Dictionary<string, string> printouts = new();
        printouts.Add("King damage", string.Format("+{0}", KING_DAMAGE_BONUS));
        return printouts;
    }
}
