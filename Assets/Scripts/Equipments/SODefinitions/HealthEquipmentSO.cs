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

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Health", String.Format("{0}", this.HealthOffset));       

        return toPrint;
    }
}
