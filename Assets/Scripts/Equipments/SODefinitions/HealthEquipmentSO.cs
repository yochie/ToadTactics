using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Health", menuName = "Equipments/HealthEquipment")]
public class HealthEquipmentSO : EquipmentSO, IHealthModifier, IEquipmentQuality
{
    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }
    
    [field: SerializeField]
    public int HealthOffset { get; set; }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentMaxHealth = playerCharacter.currentStats.maxHealth;

        playerCharacter.currentStats = new CharacterStats(playerCharacter.currentStats, maxHealth: currentMaxHealth + this.HealthOffset);
        playerCharacter.TakeDamage(HealthOffset, DamageType.healing);
    }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Health", String.Format("+{0}", this.HealthOffset));       

        return toPrint;
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentMaxHealth = playerCharacter.currentStats.maxHealth;

        playerCharacter.currentStats = new CharacterStats(playerCharacter.currentStats, maxHealth: currentMaxHealth - this.HealthOffset);

        //should just apply clamping to current health
        playerCharacter.TakeDamage(0, DamageType.healing);
    }
}
