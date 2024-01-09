using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Buff is used to diplay information about ballista and apply vulnerability effect
//Actions granted by ballista are handled by MapInputHandler
[CreateAssetMenu(fileName = "BallistaBuff", menuName = "Buffs/BallistaBuff")]
public class BallistaBuffSO : ScriptableObject, IBuffDataSO, IMitigationEnhancer
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
    public Ballista BallistaPrefab;

    [field: SerializeField]
    public float PriorityOrder { get; set; }

    public string GetTooltipDescription()
    {
        return string.Format("Character can use or reload ballista for massive AOE but will take increased damage from all sources.");
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Ballista damage", Utility.DamageStatsToString(this.BallistaPrefab.damage, this.BallistaPrefab.damageIterations, this.BallistaPrefab.damageType));
        statsDictionary.Add("Ballista range", this.BallistaPrefab.range.ToString());
        statsDictionary.Add("Ballista target", IAreaTargeter.GetAreaDescription(this.BallistaPrefab.attackAreaType, this.BallistaPrefab.attackAreaScaler));
        statsDictionary.Add("Damage taken", string.Format("{0}%", this.BallistaPrefab.vulnerabilityMultiplier * 100));
        return statsDictionary;
    }

    public Hit MitigateHit(Hit hitToMitigate)
    {
        return new Hit(hitToMitigate.damage * 2, hitToMitigate.damageType, hitToMitigate.hitSource, hitToMitigate.isCrit, hitToMitigate.penetratesArmor);
    }

    public int CompareTo(IMitigationEnhancer other)
    {
        return this.PriorityOrder.CompareTo(other.PriorityOrder);
    }
}