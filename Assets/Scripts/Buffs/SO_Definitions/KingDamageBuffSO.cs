using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "KingDamageBuff", menuName = "Buffs/KingDamageBuff")]
public class KingDamageBuffSO : ScriptableObject, IBuffDataSO, IAttackEnhancer
{
    [field: SerializeField]
    public string stringID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField] 
    public bool IsPositive { get; set; }

    [field: SerializeField] 
    public DurationType DurationType { get; set; }

    [field: SerializeField] 
    public int TurnDuration { get; set; }

    [field: SerializeField] 
    public Sprite Icon { get; set; }

    [field: SerializeField]
    private int KingDamageBonus { get; set; }

    [Server]
    public IAttackAction EnhanceAttack(IAttackAction attackToEnhance)
    {

        //We are only checking if primary target is king and buffing whole attack damage. 
        //If barb attack were ever to have a different area type than single, we might want to revisit this.
        //Perhaps use some SetupTargets function inside any ability/attack action so that any modifiers get to check on all targets
        //similar to what is done for IMovementActions
        if (attackToEnhance.TargetHex.HoldsACharacter() && attackToEnhance.TargetHex.GetHeldCharacterObject().IsKing)
        {
            attackToEnhance.Damage = attackToEnhance.Damage + KingDamageBonus;
        }
        return attackToEnhance;
    }

    public string GetTooltipDescription()
    {
        return string.Format("Grants bonus damage when attacking a king.");
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("King damage", string.Format("+{0}", this.KingDamageBonus));
        if(this.DurationType != DurationType.eternal)
            statsDictionary.Add("Duration", IBuffDataSO.GetDurationDescritpion(this));
        return statsDictionary;
    }
}
