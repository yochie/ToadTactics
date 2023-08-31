using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecroDOTEffect : IAppliablBuff
{

    //public string BuffTypeID => "NecroDOTData";
    //public string UIName => "Rotting Corpse";
    //public string IconName => "skull";
    //public bool IsPositive => false;
    //public bool NeedsToBeReAppliedEachTurn => true;

    private int DOTdamage = 10;
    private DamageType DOTDamageType = DamageType.magic;

    public bool NeedsToBeReAppliedEachTurn => throw new NotImplementedException();

    public string stringID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string UIName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsPositive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DurationType DurationType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int TurnDuration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Image Icon { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string tooltipDescription { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #region IBuffEffect functions
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
    #endregion
}
