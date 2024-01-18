using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsTable : MonoBehaviour
{

    [SerializeField]
    private GameObject statNameAsIconPrefab;

    [SerializeField]
    private GameObject statNameAsStringPrefab;

    [SerializeField]
    private GameObject statValueLabelPrefab;

    [SerializeField]
    private GameObject statsNameTable;
    
    [SerializeField]
    private GameObject statsValueTable;

    private List<GameObject> children = new();


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
            GameObject statNameObject = Instantiate(this.statNameAsIconPrefab, this.statsNameTable.gameObject.transform);
            this.children.Add(statNameObject);
            StatTitle statName = statNameObject.GetComponent<StatTitle>();
            statName.InitForStat(stat.Key);
            //TextMeshProUGUI nameLabel = statNameObject.GetComponent<TextMeshProUGUI>();
            //nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            this.children.Add(valueLabelObject);
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.gameObject.transform.GetComponent<RectTransform>()); ;
    }

    //Used for displaying arbitrary stats tables containing any sort of info
    internal void RenderFromDictionary(Dictionary<string, string> stats)
    {
        this.Clear();
        foreach (KeyValuePair<string, string> stat in stats)
        {
            GameObject nameLabelObject = Instantiate(this.statNameAsStringPrefab, this.statsNameTable.gameObject.transform);
            this.children.Add(nameLabelObject);

            TextMeshProUGUI nameLabel = nameLabelObject.GetComponentInChildren<TextMeshProUGUI>();
            nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            this.children.Add(valueLabelObject);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponentInChildren<TextMeshProUGUI>();
            valueLabel.text = stat.Value;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.gameObject.transform.GetComponent<RectTransform>()); ;
    }

    private void Clear()
    {
        if (this.children == null)
        {
            this.children = new();
            return;
        }

        foreach (GameObject child in this.children)
        {
            Destroy(child);
        }
        this.children.Clear();
    }
}