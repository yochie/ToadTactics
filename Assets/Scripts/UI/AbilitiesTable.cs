using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilitiesTable : MonoBehaviour
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


    public void RenderForClassDefaults(CharacterClass charClass)
    {
        var toPrint = this.GetAbilityPrintData(charClass);
        this.RenderFromPrintData(toPrint);
    }

    public void RenderForLiveCharacter(PlayerCharacter character)
    {
        var toPrint = this.GetAbilityPrintData(character.charClass, live: true, liveCharacter: character);
        this.RenderFromPrintData(toPrint, live: true);
    }

    private List<AbilityPrintData> GetAbilityPrintData(CharacterClass charClass, bool live = false, PlayerCharacter liveCharacter = null)
    {
        List<AbilityPrintData> toPrint = new();

        //return empty list if no abilities
        if (charClass.abilities == null)
            return toPrint;

        foreach (CharacterAbilityStats ability in charClass.abilities)
        {

            string name = ability.interfaceName;
            string description = ability.description;
            string damageOneLiner = Utility.DamageStatsToString(ability.damage, ability.damageIterations, ability.damageType);
            string range = ability.range == -1 ? "" : ability.range.ToString();
            string aoe = ability.aoe == -1 ? "" : ability.aoe.ToString();
            string usesPerRound = ability.usesPerRound == -1 ? "" : ability.usesPerRound.ToString();
            string cooldownDuration = ability.cooldownDuration == -1 ? "" : ability.cooldownDuration.ToString();
            string passiveOrActive = ability.isPassive ? "Passive" : "Active";
            string currentCooldownString = "";
            string remainingUsesString = "";

            if (live)
            {
                if (ability.isPassive)
                {
                    currentCooldownString = "";
                    remainingUsesString = "";

                }
                else
                {
                    if (ability.cappedPerRound)
                    {
                        int remainingUses = liveCharacter.GetAbilityUsesRemaining(ability.stringID);
                        remainingUsesString = string.Format("{0} uses left", remainingUses);
                    }
                    if (ability.cappedByCooldown)
                    {
                        int currentCooldown = liveCharacter.GetAbilityCooldown(ability.stringID);
                        currentCooldownString = currentCooldown.ToString();
                    }
                }
            }

            toPrint.Add(new AbilityPrintData(name: name,
                                             description: description,
                                             passiveOrActive: passiveOrActive,
                                             damageOneLiner: damageOneLiner,
                                             range: range,
                                             aoe: aoe,
                                             usesPerRound: usesPerRound,
                                             cooldownDuration: cooldownDuration,
                                             currentCooldown: currentCooldownString,                                             
                                             currentRemainingUses: remainingUsesString,
                                             damageColor: Color.black));
        }

        return toPrint;
    }

    private void RenderFromPrintData(List<AbilityPrintData> printData, bool live = false)
    {
        this.Clear();
        foreach(AbilityPrintData abilityPrintData in printData)
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
        }

        this.cooldownsColumn.SetActive(live);
    }

    private void Clear()
    {
        foreach (TextMeshProUGUI child in this.nameColumn.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }

        foreach (TextMeshProUGUI child in this.descriptionColumn.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Destroy(child.gameObject);
        }

        foreach (CooldownIndicator child in this.cooldownsColumn.GetComponentsInChildren<CooldownIndicator>())
        {
            Destroy(child.gameObject);
        }
    }
}
