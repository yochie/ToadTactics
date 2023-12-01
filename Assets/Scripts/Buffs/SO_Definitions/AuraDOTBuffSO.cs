using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AuraDOTBuff", menuName = "Buffs/AuraDOTBuff")]
public class AuraDOTBuffSO : ScriptableObject, IAppliablBuffDataSO, IAreaTargeter
{

    [field: SerializeField]
    public string stringID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField]
    private int DOTDamage { get; set; }

    [field: SerializeField]
    private DamageType DOTDamageType { get; set; }

    [field: SerializeField]
    public AreaType TargetedAreaType { get; set; }

    [field: SerializeField]
    private List<TargetType> AuraAppliesTo {get; set;}

    [field: SerializeField]
    public int AreaScaler { get; set; }

    [field: SerializeField]
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    [field: SerializeField]
    public bool IsPositive { get; set; }

    [field: SerializeField]
    public DurationType DurationType { get; set; }

    [field: SerializeField]
    public int TurnDuration { get; set; }

    [field: SerializeField]
    public Sprite Icon { get; set; }


    public void Apply(List<int> applyToCharacterIDs, bool isReapplication)
    {
        if (!this.NeedsToBeReAppliedEachTurn)
            throw new Exception("DOT buff must have reapplication enabled.");

        if (!isReapplication)
            return;

        Debug.Log("Reapplying aura DOT effect.");

        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            var hexGrid = Map.Singleton.hexGrid;
            Hex characterHex = Map.GetHex(hexGrid, Map.Singleton.characterPositions[affectedCharacter.CharClassID]);
            List<Hex> hexesInAura = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, characterHex, characterHex, this.AreaScaler);
            foreach(Hex hex in hexesInAura)
            {
                if (!hex.HoldsACharacter() ||
                    !ActionExecutor.IsValidTargetType(affectedCharacter, hex, this.AuraAppliesTo))
                    continue;                
                PlayerCharacter characterInAura = hex.GetHeldCharacterObject();
                HitSource hitSource = DOTDamageType == DamageType.healing ? HitSource.Buff : HitSource.Debuff;
                characterInAura.TakeDamage(new Hit(this.DOTDamage, DOTDamageType, hitSource));
            }            
        }
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        //nothing to do
        return;
    }

    public string GetTooltipDescription()
    {
        string damageOrHeal = this.DOTDamageType == DamageType.healing ? "Heals" : "Deals damage to";
        return string.Format("{0} nearby characters at the end of turn.", damageOrHeal);
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        string healOrDamage = this.DOTDamageType == DamageType.healing ? "Heal" : "Damage";
        statsDictionary.Add(healOrDamage, string.Format("{0} {1}", this.DOTDamage, this.DOTDamageType));
        statsDictionary.Add("Aura area", IAreaTargeter.GetAreaDescription(this.TargetedAreaType, this.AreaScaler));
        return statsDictionary;
    }
}
