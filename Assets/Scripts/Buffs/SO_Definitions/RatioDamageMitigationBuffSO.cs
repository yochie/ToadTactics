using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[CreateAssetMenu(fileName = "RatioDamageMitigationBuff", menuName = "Buffs/RatioDamageMitigationBuff")]

public class RatioDamageMitigationBuffSO : ScriptableObject, IBuffDataSO, IMitigationEnhancer
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
    public float RatioMitigated { get; set; }

    [field: SerializeField]
    public DamageType AppliesToDamageType { get; set; }

    //Lower means applies first
    [field: SerializeField]
    public float PriorityOrder { get; set; }

    public string GetTooltipDescription()
    {
        return "Reduces damage taken by some %.";
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Mitigates", string.Format("{0}%", this.RatioMitigated*100));
        statsDictionary.Add("Damage type", string.Format("{0}",this.AppliesToDamageType));
        return statsDictionary;
    }

    public Hit MitigateHit(Hit hitToMitigate)
    {
        if (!(hitToMitigate.damageType == this.AppliesToDamageType))
            return hitToMitigate;

        return new Hit(Convert.ToInt32(hitToMitigate.damage * this.RatioMitigated), hitToMitigate.damageType, hitToMitigate.penetratesArmor);
    }

    public int CompareTo(IMitigationEnhancer other)
    {
        return this.PriorityOrder.CompareTo(other.PriorityOrder);
    }
}
