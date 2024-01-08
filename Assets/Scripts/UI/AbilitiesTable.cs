using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class AbilitiesTable : MonoBehaviour
{

    [SerializeField]
    private GameObject abilityTableEntryPrefab;

    public void RenderForClassDefaults(CharacterClass charClass)
    {
        var toPrint = this.GetAbilityPrintData(charClass);
        this.RenderFromPrintData(toPrint);
    }

    public void RenderForLiveCharacter(PlayerCharacter character, bool withCooldowns)
    {
        var toPrint = this.GetAbilityPrintData(character.charClass, live: true, liveCharacter: character, withCooldowns);
        this.RenderFromPrintData(toPrint, forLiveCharacter: true);
    }

    //eventually, we might want live characters to display current ability stats (e.g. if they are modified by equipment)
    //but currently having the character displayed as live makes no difference other than being required for cooldown parameter to take effect
    private List<AbilityPrintData> GetAbilityPrintData(CharacterClass charClass, bool live = false, PlayerCharacter liveCharacter = null, bool withCooldowns = false)
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
            if (live && withCooldowns)
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
            
            Dictionary<string, string> mainStatsDictionary = this.GenerateAbilityMainStatsDictionary(ability);
            Dictionary<string, string> passiveStatsDictionary = this.GeneratePassiveStatsDictionary(ability);
            //merge passive stats to main stats
            //will throw error if shared keys, so make sure you define printables with unique field keys
            //would be confusing to read otherwise
            passiveStatsDictionary.ToList().ForEach(entry => mainStatsDictionary.Add(entry.Key, entry.Value));

            Dictionary<string, string> buffStatsDictionary = this.GenerateAppliedBuffStatsDictionary(ability);


            IBuffDataSO buff = ability.GetActivatedBuff();
            string buffOrDebuff = "";
            string appliedBuffName = "";
            if (buff != null)
            {
                buffOrDebuff = buff.IsPositive ? "Buff" : "Debuff";
                appliedBuffName = buff.UIName;
            }
           
            toPrint.Add(new AbilityPrintData(name: name,
                                             description: description,
                                             passiveOrActive: passiveOrActive,
                                             currentCooldown: currentCooldownString,                                           
                                             currentRemainingUses: remainingUsesString,
                                             buffOrDebuff: buffOrDebuff,
                                             statsDictionary: mainStatsDictionary,
                                             appliedBuffName: appliedBuffName,
                                             buffsDictionary: buffStatsDictionary));
        }

        return toPrint;
    }

    //for passives, we want to include either self buff stats or alt attack/move stats to main tooltip
    //we dont actually want the difference to be displayed as its irrelevant to user
    private Dictionary<string, string> GeneratePassiveStatsDictionary(CharacterAbilityStats ability)
    {
        Dictionary<string, string> toReturn = new();

        if (!ability.isPassive)
            return toReturn;

        if (ability.appliesSelfBuffOnRoundStart != null)
        {
            var buffStats = ability.GetPassiveBuff().GetBuffStatsDictionary();
            buffStats.ToList().ForEach(entry => toReturn.Add(entry.Key, entry.Value));
        } 
        
        if (ability.passiveGrantsAltAttack != null)
        {
            Type actionType = ClassDataSO.Singleton.GetAttackActionTypeByID(ability.passiveGrantsAltAttack);
            IPrintableStats printableAttack = Activator.CreateInstance(actionType) as IPrintableStats;
            if (printableAttack == null)
                throw new Exception("alt attack doesn't implement IPrintable. This is required for tooltips.");
            var attackStats = printableAttack.GetStatsDictionary();
            attackStats.ToList().ForEach(entry => toReturn.Add(entry.Key, entry.Value));
        }

        if (ability.passiveGrantsAltMove != null)
        {
            Type actionType = ClassDataSO.Singleton.GetMoveActionTypeByID(ability.passiveGrantsAltMove);
            IPrintableStats printableMove = Activator.CreateInstance(actionType) as IPrintableStats;
            if (printableMove == null)
                throw new Exception("alt move doesn't implement IPrintable. This is required for tooltips.");
            var moveStats = printableMove.GetStatsDictionary();
            moveStats.ToList().ForEach(entry => toReturn.Add(entry.Key, entry.Value));
        }

        return toReturn;
    }

    private Dictionary<string, string> GenerateAppliedBuffStatsDictionary(CharacterAbilityStats ability)
    {
        Dictionary<string, string> buffStatsDictionary = new();
        IBuffDataSO buff = ability.GetActivatedBuff();
        if(buff != null)
            buffStatsDictionary = buff.GetBuffStatsDictionary();
        return buffStatsDictionary;
    }

    private Dictionary<string, string> GenerateAbilityMainStatsDictionary(CharacterAbilityStats ability)
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

        string usesPerRound = ability.usesPerRound == -1 ? "" : string.Format("{0}/round", ability.usesPerRound);
        if (usesPerRound != "")
            abilityStatsDictionary.Add("Uses", usesPerRound);

        string cooldownDuration = ability.cooldownDuration == -1 ? "" : ability.cooldownDuration.ToString();
        if (cooldownDuration != "")
            abilityStatsDictionary.Add("Cooldown", string.Format("{0} turns",cooldownDuration));

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
