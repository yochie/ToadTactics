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
    public List<DamageType> AppliesToDamageType { get; set; }

    //Lower means applies first
    [field: SerializeField]
    public float PriorityOrder { get; set; }

    public string GetTooltipDescription()
    {
        return "Reduces damage taken by some %.";
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        string damageTypeString = this.AppliesToDamageType[0].ToString();
        if(AppliesToDamageType.Count > 1)
        {
            bool first = true;
            foreach(DamageType dmgType in this.AppliesToDamageType)
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                damageTypeString += string.Format(", {0}", dmgType);
            }
        }
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Mitigates", string.Format("{0}%", this.RatioMitigated*100));
        statsDictionary.Add("Damage type", string.Format("{0}", damageTypeString));
        return statsDictionary;
    }

    public Hit MitigateHit(Hit hitToMitigate)
    {
        if (!(this.AppliesToDamageType.Contains(hitToMitigate.damageType)))
            return hitToMitigate;

        return new Hit(hitToMitigate.damage - ((int) Math.Round((hitToMitigate.damage * this.RatioMitigated))), hitToMitigate.damageType, hitToMitigate.penetratesArmor);
    }

    public int CompareTo(IMitigationEnhancer other)
    {
        return this.PriorityOrder.CompareTo(other.PriorityOrder);
    }
}
