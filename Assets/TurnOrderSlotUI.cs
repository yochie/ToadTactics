using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnOrderSlotUI : MonoBehaviour
{
    public int holdsPrefabWithIndex = -1;

    private string initiativeLabel;
    public string InitiativeLabel
    {
        get { return this.initiativeLabel; }
        set {
            this.initiativeLabel = value;
            this.GetComponentInChildren<TextMeshProUGUI>().text = value;  
        }
    }

}
