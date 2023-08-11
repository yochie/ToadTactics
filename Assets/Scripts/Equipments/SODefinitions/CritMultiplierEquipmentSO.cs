using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CritMultiplier", menuName = "Equipments/CritMultiplierEquipment")]
public class CritMultiplierSO : EquipmentSO, IEquipmentQuality, ICritMultiplierModifier
{
    [field: SerializeField]
    public float CritMultiplierOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        float currentCritMultiplier = playerCharacter.CurrentStats.critMultiplier;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, critMultiplier: currentCritMultiplier + this.CritMultiplierOffset));
    }
    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        float currentCritMultiplier = playerCharacter.CurrentStats.critMultiplier;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, critMultiplier: currentCritMultiplier - this.CritMultiplierOffset));
    }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Crit multi", System.String.Format("+{0}%", this.CritMultiplierOffset * 100));

        return toPrint;
    }
}
