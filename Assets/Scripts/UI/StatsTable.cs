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

    public Dictionary<string, string> GetPrintableStatsDictionary(CharacterStats stats)
    {
        Dictionary<string, string> toPrint = new();

        string formattedDamageType;
        switch (stats.damageType)
        {
            case DamageType.physical:
                formattedDamageType = "phys";
                break;
            case DamageType.healing:
                formattedDamageType = "heal";
                break;
            default:
                formattedDamageType = stats.damageType.ToString();
                break;
        }

        toPrint.Add("Health", String.Format("{0}", stats.maxHealth));
        toPrint.Add("Armor", String.Format("{0}", stats.armor));
        toPrint.Add("Damage", String.Format("{0} x {1} ({2})", stats.damage, stats.damageIterations, formattedDamageType));
        toPrint.Add("Crit", String.Format("{0}% (+{1}%)", stats.critChance * 100, stats.critMultiplier * 100));
        toPrint.Add("Range", String.Format("{0}", stats.range));
        toPrint.Add("Moves", String.Format("{0}", stats.moveSpeed));
        toPrint.Add("Initiative", String.Format("{0}", stats.initiative));

        return toPrint;
    }

    internal void RenderForEquipment(IStatModifier statEquipment)
    {
        var toPrint = statEquipment.GetPrintableStatDictionary();
        this.RenderFromDictionary(toPrint, false, true);
    }

    public void RenderForBaseStats(CharacterStats stats, bool isAKing)
    {
        var toPrint = this.GetPrintableStatsDictionary(stats);
        this.RenderFromDictionary(stats: toPrint, isAKing : isAKing, baseStats: true);
    }

    public void RenderForCurrentStats(CharacterStats stats, bool isAKing)
    {
        var toPrint = this.GetPrintableStatsDictionary(stats);
        this.RenderFromDictionary(stats: toPrint, isAKing: isAKing, baseStats: false);
    }

    internal void RenderFromDictionary(Dictionary<string, string> stats, bool isAKing, bool baseStats)
    {
        this.Clear();
        foreach (KeyValuePair<string, string> stat in stats)
        {
            GameObject nameLabelObject = Instantiate(this.statNameLabelPrefab, this.statsNameTable.gameObject.transform);
            TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
            nameLabel.text = stat.Key;

            GameObject valueLabelObject = Instantiate(this.statValueLabelPrefab, this.statsValueTable.gameObject.transform);
            TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
            if(isAKing && stat.Key == "Health")
            {
                if (baseStats)
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


    //TODO : create PrintableStat class/struct with name, value string and colors
    internal void RenderForActiveCharacter(PlayerCharacter playerCharacter)
    {
        this.Clear();
        CharacterStats currentStats = playerCharacter.CurrentStats;
        CharacterStats baseStats = playerCharacter.charClass.stats;

        throw new NotImplementedException();
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