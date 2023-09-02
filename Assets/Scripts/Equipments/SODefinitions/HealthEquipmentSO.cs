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
        int currentMaxHealth = playerCharacter.CurrentStats.maxHealth;

        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, maxHealth: currentMaxHealth + this.HealthOffset));
        playerCharacter.TakeDamage(new Hit(HealthOffset, DamageType.healing));
    }

    public Dictionary<string, string> GetStatModificationsDictionnary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Health", String.Format("+{0}", this.HealthOffset));       

        return toPrint;
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int currentMaxHealth = playerCharacter.CurrentStats.maxHealth;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, maxHealth: currentMaxHealth - this.HealthOffset));
        //clamps and triggers Die if previously alive
        playerCharacter.SetCurrentLife(playerCharacter.CurrentLife);

    }
}
