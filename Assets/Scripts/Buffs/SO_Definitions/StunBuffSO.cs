using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StunBuff", menuName = "Buffs/StunBuff")]
public class StunBuffSO : ScriptableObject, IAppliablBuffDataSO
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
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    public string GetDescription()
    {
        string durationString = IBuffDataSO.GetDurationDescritpion(this);

        return string.Format("Afflicted character cannot take any actions during his turn. {0}", durationString);
    }

    public void Apply(List<int> applyToCharacterIDs, bool isReapplication = false)
    {
        foreach(int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.SetCanTakeTurns(false);
        }
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.SetCanTakeTurns(true);
        }
    }
}