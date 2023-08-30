using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

[CreateAssetMenu]
public class BarbarianKingDamageEffect : IBuffDataSO, IAttackEnhancer
{
    #region IBuff
    [field: SerializeField]
    public string BuffTypeID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField] 
    public bool IsPositive { get; set; }

    [field: SerializeField] 
    public DurationType DurationType { get; set; }

    [field: SerializeField] 
    public int TurnDuration { get; set; }

    [field: SerializeField] 
    public Image Icon { get; set; }
    public string tooltipDescription { get; set; }
    #endregion

    [field: SerializeField]
    private int kingDamageBonus { get; set; }

    [Server]
    public IAttackAction EnhanceAttack(IAttackAction attackToEnhance)
    {

        //We are only checking if primary target is king and buffing whole attack damage. 
        //If barb attack were ever to have a different area type than single, we might want to revisit this.
        //Perhaps use some SetupTargets function inside any ability/attack action so that any modifiers get to check on all targets
        //similar to what is done for IMovementActions
        if (attackToEnhance.TargetHex.HoldsACharacter() && attackToEnhance.TargetHex.GetHeldCharacterObject().IsKing)
        {
            attackToEnhance.Damage = attackToEnhance.Damage + kingDamageBonus;
        }
        return attackToEnhance;
    }

    public Dictionary<string, string> GetAbilityBuffPrintoutDictionnary()
    {
        Dictionary<string, string> printouts = new();
        printouts.Add("King damage", string.Format("+{0}", kingDamageBonus));
        return printouts;
    }
}
