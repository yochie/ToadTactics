using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Range", menuName = "Equipments/RangeEquipment")]
public class RangeEquipmentSO : EquipmentSO, IEquipmentQuality, IRangeModifier
{
    [field: SerializeField]
    public int RangeOffset { get; set; }

    [field: SerializeField]
    public EquipmentQuality Quality { get; set; }

    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Range", System.String.Format("{0}", this.RangeOffset));

        return toPrint;
    }
}
