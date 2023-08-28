using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage", menuName = "Equipments/DamageEquipment")]
public class DamageEquipmentSO : EquipmentSO, IEquipmentQuality, IDamageModifier
{
    [field: SerializeField]
    public int DamageOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentDamage = playerCharacter.CurrentStats.damage;

        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, damage: currentDamage + this.DamageOffset));
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentDamage = playerCharacter.CurrentStats.damage;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, damage: currentDamage - this.DamageOffset));
    }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Damage", System.String.Format("+{0}", this.DamageOffset));

        return toPrint;
    }
}
