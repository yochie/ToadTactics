using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DOTBuff", menuName = "Buffs/DOTBuff")]
public class DOTBuff : ScriptableObject, IAppliablBuff
{

    //public string BuffTypeID => "NecroDOTData";
    //public string UIName => "Rotting Corpse";
    //public string IconName => "skull";
    //public bool IsPositive => false;
    //public bool NeedsToBeReAppliedEachTurn => true;
    //private int DOTdamage = 10;
    //private DamageType DOTDamageType = DamageType.magic;

    [field: SerializeField]
    private int DOTdamage { get; set; }
    
    [field: SerializeField]
    private DamageType DOTDamageType { get; set; }

    [field: SerializeField]
    public bool NeedsToBeReAppliedEachTurn { get; set; }
    
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

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication)
    {
        if (!isReapplication)
            return false;

        Debug.Log("Reapplying Necro DOT effect.");

        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.TakeDamage(DOTdamage, DOTDamageType);
        }
        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        //nothing to do
        return;
    }

    public string GetDescription()
    {
        
        string durationString = IBuffDataSO.GetDurationDescritpion(this);
        
        return string.Format("Afflicted character takes {0} {1} damage at the end of their turn. {2}", this.DOTdamage, this.DOTDamageType, durationString);
    }
}
