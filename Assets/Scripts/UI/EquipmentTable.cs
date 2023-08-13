using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentTable : MonoBehaviour
{
    private List<Image> displayedIcons = new();

    [SerializeField]
    private Image blankIconPrefab;

    public void RenderWithEquipments(List<string> equipmentIDs)
    {

        this.Clear();

        foreach (string equipmentID in equipmentIDs)
        {
            Image equipmentICon = Instantiate(this.blankIconPrefab, this.transform);
            this.displayedIcons.Add(equipmentICon);
            equipmentICon.sprite = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID).Sprite;
            equipmentICon.color = Color.black;
        }
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