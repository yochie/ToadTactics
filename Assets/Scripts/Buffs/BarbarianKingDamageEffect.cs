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

        //We are only checking if primary target is king and buffing whole attack damage. 
        //If barb attack were ever to have a different area type than single, we might want to revisit this.
        //Perhaps use some SetupTargets function inside any ability/attack action so that any modifiers get to check on all targets
        //similar to what is done for IMovementActions
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
