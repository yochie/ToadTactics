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
        this.displayedIcons.Add(equipmentIConContainer);
        EquipmentIcon equipmentIcon = equipmentIConContainer.GetComponent<EquipmentIcon>();

        equipmentIcon.Init(equipmentID);
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