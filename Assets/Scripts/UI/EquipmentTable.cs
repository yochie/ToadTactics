using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class EquipmentTable : MonoBehaviour
{
    private List<Image> displayedIcons = new();

    [SerializeField]
    private Image blankIconPrefab;

    public void SetupWithEquipments(List<string> equipmentIDs)
    {

        this.Clear();

        foreach (string equipmentID in equipmentIDs)
        {
            this.AddEquipment(equipmentID);
        }
    }
    public void AddEquipment(string equipmentID)
    {
        Image equipmentICon = Instantiate(this.blankIconPrefab, this.transform);
        this.displayedIcons.Add(equipmentICon);
        equipmentICon.sprite = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID).Sprite;
        equipmentICon.color = Color.black;
    }

    private void Clear()
    {
        foreach(Image icon in this.displayedIcons)
        {
            Destroy(icon.gameObject);
        }
        this.displayedIcons.Clear();
    }


}