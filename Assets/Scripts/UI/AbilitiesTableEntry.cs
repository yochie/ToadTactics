using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AbilitiesTableEntry : MonoBehaviour
{
    [SerializeField]
    private GameObject nameLabelPrefab;

    [SerializeField]
    private GameObject descriptionLabelPrefab;

    [SerializeField]
    private GameObject cooldownIndicatorPrefab;

    [SerializeField]
    private GameObject nameColumn;

    [SerializeField]
    private GameObject descriptionColumn;

    [SerializeField]
    private GameObject cooldownsColumn;

    [SerializeField]
    private GameObject tooltip;

    [SerializeField]
    private StatsTable tooltipStats;

    [SerializeField]
    private TextMeshProUGUI tooltipTitle;

    internal void Init(AbilityPrintData abilityPrintData, bool forLiveCharacter)
    {
        GameObject nameLabelObject = Instantiate(this.nameLabelPrefab, this.nameColumn.transform);
        TextMeshProUGUI nameLabel = nameLabelObject.GetComponent<TextMeshProUGUI>();
        nameLabel.text = abilityPrintData.name;

        GameObject valueLabelObject = Instantiate(this.descriptionLabelPrefab, this.descriptionColumn.transform);
        TextMeshProUGUI valueLabel = valueLabelObject.GetComponent<TextMeshProUGUI>();
        valueLabel.text = abilityPrintData.description;

        GameObject cooldownIndicatorObject = Instantiate(this.cooldownIndicatorPrefab, this.cooldownsColumn.transform);
        CooldownIndicator cooldownIndicator = cooldownIndicatorObject.GetComponent<CooldownIndicator>();
        cooldownIndicator.SetCooldown(abilityPrintData.currentCooldown);
        cooldownIndicator.SetUsesCount(abilityPrintData.currentRemainingUses);
        this.cooldownsColumn.SetActive(forLiveCharacter);

        this.tooltipTitle.text = abilityPrintData.passiveOrActive;

        Dictionary<string, string> abilityStatsForTooltipTable = new();
        if(abilityPrintData.damageOneLiner != "")
            abilityStatsForTooltipTable.Add("Damage", abilityPrintData.damageOneLiner);
        
        if(abilityPrintData.range != "")
            abilityStatsForTooltipTable.Add("Range", abilityPrintData.range);

        if (abilityPrintData.aoe != "")
            abilityStatsForTooltipTable.Add("AOE", abilityPrintData.aoe);

        if (abilityPrintData.usesPerRound != "")
            abilityStatsForTooltipTable.Add("Uses", abilityPrintData.usesPerRound);

        if (abilityPrintData.cooldownDuration != "")
            abilityStatsForTooltipTable.Add("Cooldown", abilityPrintData.cooldownDuration);

        if(abilityStatsForTooltipTable.Count == 0)
        {
            this.tooltipStats.gameObject.SetActive(false);
        }
        else
        {
            this.tooltipStats.gameObject.SetActive(true);
            this.tooltipStats.RenderFromDictionary(abilityStatsForTooltipTable);
        }

    }
}
