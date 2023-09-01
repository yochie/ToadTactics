using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilitiesTable : MonoBehaviour
{

    [SerializeField]
    private GameObject abilityTableEntryPrefab;

    public void RenderForClassDefaults(CharacterClass charClass)
    {
        var toPrint = this.GetAbilityPrintData(charClass);
        this.RenderFromPrintData(toPrint);
    }

    public void RenderForLiveCharacter(PlayerCharacter character)
    {
        var toPrint = this.GetAbilityPrintData(character.charClass, live: true, liveCharacter: character);
        this.RenderFromPrintData(toPrint, forLiveCharacter: true);
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
                        remainingUsesString = string.Format("{0} left", remainingUses);
                    }
                    if (ability.cappedByCooldown)
                    {
                        int currentCooldown = liveCharacter.GetAbilityCooldown(ability.stringID);
                        currentCooldownString = currentCooldown.ToString();
                    }
                }
            }
            
            Dictionary<string, string> abilityStatsDictionary = this.GenerateAbilityStatsDictionary(ability);
            
            Dictionary<string, string> buffStatsDictionary = this.GenerateBuffStatsDictionary(ability);

            IBuffDataSO buff = ability.GetAppliedBuf();
            string buffOrDebuff = "";
            if (buff != null)
                buffOrDebuff = buff.IsPositive ? "Buff" : "Debuff";

            toPrint.Add(new AbilityPrintData(name: name,
                                             description: description,
                                             passiveOrActive: passiveOrActive,
                                             currentCooldown: currentCooldownString,                                           
                                             currentRemainingUses: remainingUsesString,
                                             buffOrDebuff: buffOrDebuff,
                                             statsDictionary: abilityStatsDictionary,
                                             buffsDictionary: buffStatsDictionary));
        }

        return toPrint;
    }

    private Dictionary<string, string> GenerateBuffStatsDictionary(CharacterAbilityStats ability)
    {
        Dictionary<string, string> buffStatsDictionary = new();
        IBuffDataSO buff = ability.GetAppliedBuf();
        if(buff != null)
            buffStatsDictionary = buff.GetBuffStatsDictionary();
        return buffStatsDictionary;
    }

    private Dictionary<string, string> GenerateAbilityStatsDictionary(CharacterAbilityStats ability)
    {
        Dictionary<string, string> abilityStatsDictionary = new();
        string damageOneLiner = Utility.DamageStatsToString(ability.damage, ability.damageIterations, ability.damageType);
        if (damageOneLiner != "")
            abilityStatsDictionary.Add("Damage", damageOneLiner);

        string range;
        if (ability.range == Utility.MAX_DISTANCE_ON_MAP)
            range = "infinite";
        else
            range = !(ability.range > 0) ? "" : ability.range.ToString();
        if (range != "")
            abilityStatsDictionary.Add("Range", range);

        string areaOneLiner = IAreaTargeter.GetAreaDescription(ability.areaType, ability.areaScaler);
        if (areaOneLiner != "")
            abilityStatsDictionary.Add("Target", areaOneLiner);

        string usesPerRound = ability.usesPerRound == -1 ? "" : string.Format("{0} per round", ability.usesPerRound);
        if (usesPerRound != "")
            abilityStatsDictionary.Add("Uses", usesPerRound);

        string cooldownDuration = ability.cooldownDuration == -1 ? "" : ability.cooldownDuration.ToString();
        if (cooldownDuration != "")
            abilityStatsDictionary.Add("Cooldown", cooldownDuration);

        return abilityStatsDictionary;
    }

    private void RenderFromPrintData(List<AbilityPrintData> printData, bool forLiveCharacter = false)
    {
        this.Clear();
        foreach(AbilityPrintData abilityPrintData in printData)
        {

            GameObject abilityTableEntry = Instantiate(this.abilityTableEntryPrefab, this.transform);
            abilityTableEntry.GetComponent<AbilitiesTableEntry>().Init(abilityPrintData, forLiveCharacter);            
        }

    }

    private void Clear()
    {
        foreach (AbilitiesTableEntry entry in this.GetComponentsInChildren<AbilitiesTableEntry>())
        {
            Destroy(entry.gameObject);
        }
    }
}
