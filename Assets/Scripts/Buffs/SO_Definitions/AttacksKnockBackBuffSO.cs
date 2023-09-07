using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AttacksKnockBackBuff", menuName = "Buffs/AttacksKnockBackBuff")]
public class AttacksKnockBackBuffSO : ScriptableObject, IBuffDataSO, IAttackEnhancer
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
    private int MaxAttackRange { get; set; }

    [field: SerializeField]
    private int KnockbackDistance { get; set; }

    [field: SerializeField]
    private int BonusDamage { get; set; }

    [Server]
    public IAttackAction EnhanceAttack(IAttackAction attackToEnhance)
    {
        //only checking primary target..
        if(MapPathfinder.HexDistance(attackToEnhance.ActorHex, attackToEnhance.TargetHex) <= this.MaxAttackRange)
        {            
            attackToEnhance.Knockback += this.KnockbackDistance;
            attackToEnhance.Damage += this.BonusDamage;
        }
        return attackToEnhance;
    }

    public string GetTooltipDescription()
    {
        return "Grants knockback and increased damage at some range.";
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Knockback", string.Format("{0}", this.KnockbackDistance));
        statsDictionary.Add("Damage", string.Format("+{0}", this.BonusDamage));
        statsDictionary.Add("At range", string.Format("{0}", this.MaxAttackRange));
        return statsDictionary;
    }
}
