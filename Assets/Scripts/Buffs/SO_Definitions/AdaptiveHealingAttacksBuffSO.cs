using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AdaptiveHealingAttacksBuff", menuName = "Buffs/AdaptiveHealingAttacksBuff")]
public class AdaptiveHealingAttacksBuffSO : ScriptableObject, IBuffDataSO, IAttackEnhancer
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
    public float DamageToHealRatio { get; set; }

    [field: SerializeField]
    public DamageType DamageConversion { get; set; }

    [Server]
    public IAttackAction EnhanceAttack(IAttackAction attackToEnhance)
    {
        //only checking primary target to determine friendly fire since attack has single damage type
        if (!attackToEnhance.TargetHex.HoldsACharacter())
            return attackToEnhance;

        if(attackToEnhance.TargetHex.GetHeldCharacterObject().OwnerID == attackToEnhance.ActorCharacter.OwnerID)
        {
            attackToEnhance.AttackDamageType = DamageType.healing;
        } else
        {
            attackToEnhance.AttackDamageType = this.DamageConversion;
            attackToEnhance.Damage = (int) Math.Round(attackToEnhance.Damage * this.DamageToHealRatio, 0);
        }

        return attackToEnhance;
    }

    public string GetTooltipDescription()
    {
        return "Attacks heal allies but damage enemies to lesser extent.";
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Damage ratio", this.DamageToHealRatio.ToString());
        statsDictionary.Add("Damage type", Utility.FormattedDamageType(this.DamageConversion));

        return statsDictionary;
    }
}
