using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatTitle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameLabel;
    
    [SerializeField]
    private Image icon;
    
    [SerializeField]
    private StatIconsDataSO statIconsData;

    public void InitForStat(string statName)
    {
        this.icon.sprite = this.statIconsData.GetSpriteForStat(statName);
        this.nameLabel.text = statName;
    }

}
