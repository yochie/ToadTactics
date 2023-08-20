using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class EquipmentTable : MonoBehaviour
{
    private List<GameObject> displayedIcons = new();

    [SerializeField]
    private GameObject blankIconContainerPrefab;

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
        GameObject equipmentIConContainer = Instantiate(this.blankIconContainerPrefab, this.transform);
        Image equipmentIcon = equipmentIConContainer.GetComponentInChildren<Image>();
        this.displayedIcons.Add(equipmentIConContainer);
        equipmentIcon.sprite = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID).Sprite;
        equipmentIcon.color = Color.black;
    }

    private void Clear()
    {
        foreach(GameObject iconContainer in this.displayedIcons)
        {
            Destroy(iconContainer.gameObject);
        }
        this.displayedIcons.Clear();
    }


}