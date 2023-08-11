using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Initiative", menuName = "Equipments/InitiativeEquipment")]
public class InitiativeEquipmentSO : EquipmentSO, IEquipmentQuality, IInitiativeModifier
{
    [field: SerializeField]
    public float InitiativeOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        float currentInitiative = playerCharacter.CurrentStats.initiative;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, initiative: currentInitiative + this.InitiativeOffset));
    }
    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        float currentInitiative = playerCharacter.CurrentStats.initiative;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, initiative: currentInitiative - this.InitiativeOffset));
    }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();
        if(this.InitiativeOffset > 0)
            toPrint.Add("Initiative", System.String.Format("+{0}", this.InitiativeOffset));
        else
            toPrint.Add("Initiative", System.String.Format("{0}", this.InitiativeOffset));


        return toPrint;
    }
}
