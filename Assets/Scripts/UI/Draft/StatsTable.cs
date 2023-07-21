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

    internal void RenderFromDictionary(Dictionary<string, string> dictionary)
    {
        foreach (KeyValuePair<string, string> ability in dictionary)
        {
            GameObject nameLabelObject = Instantiate(this.statNameLabelPrefab, this.statsNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = ability.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            valueLabel.text = ability.Value;
        }
    }
}