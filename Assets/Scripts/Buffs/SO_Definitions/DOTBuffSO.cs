using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DOTBuff", menuName = "Buffs/DOTBuff")]
public class DOTBuffSO : ScriptableObject, IAppliablBuffDataSO
{

    //public string BuffTypeID => "NecroDOTData";
    //public string UIName => "Rotting Corpse";
    //public string IconName => "skull";
    //public bool IsPositive => false;
    //public bool NeedsToBeReAppliedEachTurn => true;
    //private int DOTdamage = 10;
    //private DamageType DOTDamageType = DamageType.magic;

    [field: SerializeField]
    public string stringID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField]
    private int DOTDamage { get; set; }
    
    [field: SerializeField]
    private DamageType DOTDamageType { get; set; }

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

        Debug.Log("Reapplying DOT effect.");

        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            int prevLife = affectedCharacter.CurrentLife;
            Action<int> logMessageWithDamage = new((int rawDamage) => {

                string message = string.Format("{0}'s <b>{1}</b> {2} deals <b><color={3}>{4} {5}</color></b> to him",
                    affectedCharacter.charClass.name,
                    this.UIName,
                    this.IsPositive ? "buff" : "debuff",
                    Utility.DamageTypeToColorName(this.DOTDamageType),
                    rawDamage,
                    this.DOTDamageType,
                    affectedCharacter.charClass.name
                    );

            MasterLogger.Singleton.RpcLogMessage(message);
            });

            affectedCharacter.TakeDamage(new Hit(DOTDamage, DOTDamageType, HitSource.Debuff, isCrit: false), logMessageWithDamage);

        }
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        //nothing to do
        return;
    }

    public string GetTooltipDescription()
    {
        
        string durationString = IBuffDataSO.GetDurationDescritpion(this);
        
        return string.Format("Deals damage at the end of each turn.");
    }
    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Damage per turn", string.Format("{0} {1}", this.DOTDamage, this.DOTDamageType));
        statsDictionary.Add("Duration", IBuffDataSO.GetDurationDescritpion(this));
        return statsDictionary;
    }
}
