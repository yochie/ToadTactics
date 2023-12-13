using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureRevealPanel : AssignmentEquipmentPanelUI
{
    [SerializeField]
    private GameObject content;

    public void Show()
    {
        this.content.SetActive(true);
    }

    public void Hide()
    {
        this.content.SetActive(false);
    }
}
