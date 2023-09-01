using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CritChance", menuName = "Equipments/CritChanceEquipment")]
public class CritChanceEquipmentSO : EquipmentSO, IEquipmentQuality, ICritChanceModifier
{
    [field: SerializeField]
    public float CritChanceOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        float currentCritChance = playerCharacter.CurrentStats.critChance;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, critChance: currentCritChance + this.CritChanceOffset));
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        float currentCritChance = playerCharacter.CurrentStats.critChance;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, critChance: currentCritChance - this.CritChanceOffset));
    }

    public Dictionary<string, string> GetStatModificationsDictionnary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Critical chance", System.String.Format("+{0}%", this.CritChanceOffset * 100));

        return toPrint;
    }

}
