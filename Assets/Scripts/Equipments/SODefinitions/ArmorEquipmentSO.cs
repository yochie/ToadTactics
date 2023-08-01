using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Equipments/ArmorEquipment")]
public class ArmorEquipmentSO : EquipmentSO, IEquipmentQuality, IArmorModifier
{
    [field: SerializeField]
    public int ArmorOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentArmor = playerCharacter.currentStats.armor;
        playerCharacter.currentStats = new CharacterStats(playerCharacter.currentStats, armor: currentArmor + this.ArmorOffset);
    }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Armor", System.String.Format("+{0}", this.ArmorOffset));

        return toPrint;
    }
}
