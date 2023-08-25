using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentIcon : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI tooltipTitle;

    [SerializeField]
    private StatsTable tooltipStatsTable;

    public void Init(string equipmentID)
    {
        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);
        this.SetIcon(equipmentData.Sprite);
        this.SetColor(Color.black);
        this.tooltipTitle.text = equipmentData.NameUI;
        IStatModifier statModifier = equipmentData as IStatModifier;
        if (statModifier != null)
            this.tooltipStatsTable.RenderForEquipment(statModifier);
    }

    private void SetIcon(Sprite sprite)
    {
        this.icon.sprite = sprite;
    }

    private void SetColor(Color color)
    {
        this.icon.color = color;
    }
}
