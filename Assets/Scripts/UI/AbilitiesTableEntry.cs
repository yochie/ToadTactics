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
    private StatsTable tooltipAbilityStats;

    [SerializeField]
    private StatsTable tooltipBuffStats;

    [SerializeField]
    private TextMeshProUGUI tooltipTitle;

    [SerializeField]
    private TextMeshProUGUI buffSectionTitle;


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


        if(abilityPrintData.abilityStatsDictionary.Count == 0)
        {
            this.tooltipAbilityStats.gameObject.SetActive(false);
        }
        else
        {
            this.tooltipAbilityStats.gameObject.SetActive(true);
            this.tooltipAbilityStats.RenderFromDictionary(abilityPrintData.abilityStatsDictionary);
        }

        if (abilityPrintData.buffStatsDictionary.Count == 0)
        {
            this.tooltipBuffStats.gameObject.SetActive(false);
            this.buffSectionTitle.gameObject.SetActive(false);
        }
        else
        {
            this.tooltipBuffStats.gameObject.SetActive(true);
            this.buffSectionTitle.gameObject.SetActive(true);
            this.buffSectionTitle.text = abilityPrintData.buffOrDebuff;
            this.tooltipBuffStats.RenderFromDictionary(abilityPrintData.buffStatsDictionary);
        }
    }
}
