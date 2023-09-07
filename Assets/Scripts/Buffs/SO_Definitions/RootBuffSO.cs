using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RootBuff", menuName = "Buffs/RootBuff")]
public class RootBuffSO : ScriptableObject, IAppliablBuffDataSO
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

    public void Apply(List<int> applyToCharacterIDs, bool isReapplication = false)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(false);
        }
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter appliedTo = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            appliedTo.SetCanMove(true);
        }
    }
    public string GetTooltipDescription()
    {
        return string.Format("Prevents movement and use of movement abilities.");
    }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        statsDictionary.Add("Duration", IBuffDataSO.GetDurationDescritpion(this));
        return statsDictionary;
    }
}
