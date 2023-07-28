using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(fileName = "Range", menuName = "Equipments/RangeEquipment")]
public class RangeEquipmentSO : EquipmentSO, IEquipmentQuality, IRangeModifier
{
    [field: SerializeField]
    public int RangeOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

}
