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


    internal void RenderForStatEquipment(IStatModifier statEquipment)
    {
        var toPrint = statEquipment.GetStatModificationsDictionnary();
        this.RenderFromDictionary(toPrint);
    }

    public void RenderCharacterStatsForKingAssignment(CharacterStats stats)
    {
        var toPrint = stats.GetPrintableStatsDictionary();
        this.RenderFromDictionaryForCharacter(stats: toPrint, isAKing : false, forKingAssignment: true);
    }

    public void RenderCharacterStats(CharacterStats stats, bool isAKing)
    {
        var toPrint = stats.GetPrintableStatsDictionary();
        this.RenderFromDictionaryForCharacter(stats: toPrint, isAKing: isAKing, forKingAssignment: false);
    }

    internal void RenderFromDictionaryForCharacter(Dictionary<string, string> stats, bool isAKing, bool forKingAssignment)
    {
        this.Clear();
        foreach (KeyValuePair<string, string> stat in stats)
        {
            GameObject nameLabelObject = Instantiate(this.statNameLabelPrefab, this.statsNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            if((isAKing || forKingAssignment) && stat.Key == "Health")
            {
                if (forKingAssignment)
                {
                    string baseHealth = stat.Value;
                    string buffedHealth = (Utility.ApplyKingLifeBuff(Int32.Parse(baseHealth))).ToString();
                    string kingBaseHealthWithBuffDisplay = string.Format("{0} => {1}", baseHealth, buffedHealth);
                    valueLabel.text = kingBaseHealthWithBuffDisplay;
                } 
                else
                    valueLabel.text = stat.Value;
                valueLabel.color = Color.green;
            } else
            {
                valueLabel.text = stat.Value;
            }            
        }
    }

    internal void RenderFromDictionary(Dictionary<string, string> stats)
    {
        this.Clear();
        foreach (KeyValuePair<string, string> stat in stats)
        {
            GameObject nameLabelObject = Instantiate(this.statNameLabelPrefab, this.statsNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            valueLabel.text = stat.Value;
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