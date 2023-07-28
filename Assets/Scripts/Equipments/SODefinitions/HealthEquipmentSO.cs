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
}
