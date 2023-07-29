using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsTable : MonoBehaviour
{

    [SerializeField]
    private GameObject statNameLabelPrefab;

    [SerializeField]
    private GameObject statValueLabelPrefab;

    [SerializeField]
    private GameObject statsNameTable;
    
    [SerializeField]
    private GameObject statsValueTable;

    internal void RenderFromDictionary(Dictionary<string, string> dictionary, bool isAKing)
    {
        this.Clear();
        foreach (KeyValuePair<string, string> stat in dictionary)
        {
            GameObject nameLabelObject = Instantiate(this.statNameLabelPrefab, this.statsNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            if(isAKing && stat.Key == "Health")
            {
                valueLabel.text = (Utility.ApplyKingLifeBuff(Int32.Parse(stat.Value))).ToString();
                valueLabel.color = Color.green;
            } else
            {
                valueLabel.text = stat.Value;
            }
            
        }
    }


    private void Clear()
    {
        foreach (TextMeshProUGUI child in this.statsNameTable.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }

        foreach (TextMeshProUGUI child in this.statsValueTable.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }
    }
}