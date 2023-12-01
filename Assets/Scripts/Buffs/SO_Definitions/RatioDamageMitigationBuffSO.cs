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
        return "Reduces damage taken.";
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
        if(this.DurationType != DurationType.eternal)
            statsDictionary.Add("Duration",IBuffDataSO.GetDurationDescritpion(this));
        return statsDictionary;
    }

    public Hit MitigateHit(Hit hitToMitigate)
    {
        if (!(this.AppliesToDamageType.Contains(hitToMitigate.damageType)))
            return hitToMitigate;

        int mitigatedDamage = hitToMitigate.damage - ((int)Math.Round(hitToMitigate.damage * this.RatioMitigated));

        //make sure damage isnt negative
        mitigatedDamage = Math.Max(mitigatedDamage, 0);

        return new Hit(mitigatedDamage, hitToMitigate.damageType, hitToMitigate.hitSource, hitToMitigate.penetratesArmor);
    }

    public int CompareTo(IMitigationEnhancer other)
    {
        return this.PriorityOrder.CompareTo(other.PriorityOrder);
    }
}
