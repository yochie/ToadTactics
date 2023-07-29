using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class AssignmentEquipmentSheetUI : MonoBehaviour
{
    public string holdsEquipmentID;

    [SerializeField]
    private Image spriteImage;

    [SerializeField]
    private TextMeshProUGUI nameLabel;

    [SerializeField]
    private TextMeshProUGUI descriptionLabel;

    [SerializeField]
    private StatsTable statsTable;

    public void FillWithEquipmentData(string equipmentID)
    {
        Debug.LogFormat("Filling equipment slot with data for {0}", equipmentID);
        this.holdsEquipmentID = equipmentID;

        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);

        this.spriteImage.sprite = equipmentData.Sprite;
        this.nameLabel.text = equipmentData.NameUI;
        this.descriptionLabel.text = equipmentData.Description;

        if (typeof(IStatModifier).IsAssignableFrom(equipmentData.GetType()))
        {
            IStatModifier statEquipment = equipmentData as IStatModifier;
            this.statsTable.RenderFromDictionary(statEquipment.GetPrintableStatDictionary(), false);
        }

    }
}
