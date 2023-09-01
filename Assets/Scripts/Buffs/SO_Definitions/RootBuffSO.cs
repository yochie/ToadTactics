using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RootBuff", menuName = "Buffs/RootBuff")]
public class RootBuffSO : ScriptableObject, IAppliablBuff
{
    //public override string BuffTypeID => "WarriorRootData";
    //public override string UIName => "Warrior fear";
    //public override string IconName => "root";
    [field:SerializeField]
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
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication = false)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(false);
        }

        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(true);
        }
    }
    public string GetDescription()
    {
        return string.Format("Afflicted character cannot take move or use movement abilities.");
    }
}
