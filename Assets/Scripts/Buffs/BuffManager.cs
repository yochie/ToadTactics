using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

internal class BuffManager : NetworkBehaviour
{
    public static BuffManager Singleton { get; private set; }

    private void Awake()
    {
        Debug.Log("BuffManager awoken");
        if (BuffManager.Singleton != null)
        {
            Debug.Log("Destroying new buffmanager to avoid duplicate");
            Destroy(BuffManager.Singleton.gameObject);
            return;
        }            
        BuffManager.Singleton = this;
    }

    [Server]
    internal RuntimeBuff CreateAbilityBuff(IBuffDataSO buffData, CharacterAbilityStats abilityStats, int applyingCharacterID, List<int> affectedCharacterIDs)
    {
        RuntimeBuff runtimeBuff = new RuntimeBuff();
        runtimeBuff.UniqueID = IDGenerator.GetNewID();
        runtimeBuff.AffectedCharacterIDs = affectedCharacterIDs;
        runtimeBuff.Data = buffData;

        RuntimeBuffAbility abilityBuffComponent = new RuntimeBuffAbility();
        abilityBuffComponent.AppliedByAbility = abilityStats;
        abilityBuffComponent.ApplyingCharacterID = applyingCharacterID;
        runtimeBuff.AddComponent(abilityBuffComponent);

        if (buffData.DurationType == DurationType.timed)
        {
            RuntimeBuffTimeout timedBuffComponent = new RuntimeBuffTimeout();
            timedBuffComponent.TurnDurationRemaining = buffData.TurnDuration + 1;
            runtimeBuff.AddComponent(timedBuffComponent);
        }

        ITriggeredBuff triggeredBuff = buffData as ITriggeredBuff;
        if (triggeredBuff != null)
        {
            RuntimeBuffTriggerCounter triggerCounter = new();
            triggerCounter.RemainingTriggers = triggeredBuff.MaxTriggers;
            runtimeBuff.AddComponent(triggerCounter);
        }

        IConditionalBuff conditionalBuff = buffData as IConditionalBuff;
        if (conditionalBuff != null)
        {
            if (conditionalBuff.DurationType != DurationType.conditional)
                throw new Exception("Conditional buff does not have duration in data set to conditional");
        }

        return runtimeBuff;
    }

    [Server]
    public void ApplyNewBuff(RuntimeBuff buff)
    {
        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.AddAffectingBuff(buff);

            //We dont want to log eternal buffs as those are what players consider as passives
            //skip logging outside of gameplay to avoid passively granted buffs also (since those buffs are applied during character placement)
            if(buff.Data.DurationType != DurationType.eternal && GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay) 
            {
                string message = string.Format("{0} applied to {1}", buff.Data.UIName, affectedCharacter.charClass.name);
                MasterLogger.Singleton.RpcLogMessage(message);
            }
        }

        RuntimeBuffAbility abilityComponent = buff.GetComponent<RuntimeBuffAbility>();
        if (abilityComponent != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharactersByID[abilityComponent.ApplyingCharacterID];
            applyingCharacter.AddOwnedBuff(buff);
        }

        IAppliablBuffDataSO appliedBuff = buff.Data as IAppliablBuffDataSO;
        if (appliedBuff != null)
        {
            appliedBuff.Apply(buff.AffectedCharacterIDs, isReapplication: false);
        }

        Sprite buffIcon = buff.Data.Icon;
        if(buffIcon != null)
        {
            RuntimeBuffTimeout timedBuffComponent = buff.GetComponent<RuntimeBuffTimeout>();
            if (timedBuffComponent != null)
                this.RpcAddBuffIcons(buff.UniqueID, buff.AffectedCharacterIDs, buff.Data.stringID, timedBuffComponent.TurnDurationRemaining);
            else
                this.RpcAddBuffIcons(buff.UniqueID, buff.AffectedCharacterIDs, buff.Data.stringID, -1);
        }

        ITriggeredBuff triggeredBuff = buff.Data as ITriggeredBuff;
        if (triggeredBuff != null)
        {
            triggeredBuff.SetupTriggerListeners(buff);
        }

        IConditionalBuff conditionalBuff = buff.Data as IConditionalBuff;
        if (conditionalBuff != null)
        {
            conditionalBuff.SetupConditionListeners(buff);
        }

    }
    
    [Server]
    public void TickBuffsForTurn(int playingCharacterID)
    {        
        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            if (character.CharClassID != playingCharacterID)
                continue;
            foreach(RuntimeBuff ownedBuff in character.ownerOfBuffs.ToArray())
            {
                RuntimeBuffTimeout timedBuffComponent = ownedBuff.GetComponent<RuntimeBuffTimeout>();
                if (timedBuffComponent == null)
                    continue;
                timedBuffComponent.TurnDurationRemaining--;
                if (timedBuffComponent.TurnDurationRemaining == 0)
                {
                    //need to copy since otherwise removing affected characters empties list before icons are removed
                    List<int> copyOfAffectedCharacters = new();
                    ownedBuff.AffectedCharacterIDs.CopyTo(copyOfAffectedCharacters);
                    this.RemoveBuffFromCharacters(ownedBuff, copyOfAffectedCharacters);
                } else
                {
                    this.RpcUpdateBuffIconDurations(ownedBuff.UniqueID, ownedBuff.AffectedCharacterIDs, ownedBuff.Data.stringID, timedBuffComponent.TurnDurationRemaining);
                }
            }

            foreach (RuntimeBuff buff in character.affectedByBuffs.ToArray())
            {
                if (buff.Data is IAppliablBuffDataSO appliableBuff && appliableBuff.NeedsToBeReAppliedEachTurn && !character.IsDead)
                {
                    appliableBuff.Apply(new List<int> { character.CharClassID }, isReapplication: true);
                    string message = string.Format("Ticking {0} {1} on {2}", appliableBuff.UIName, appliableBuff.IsPositive ? "buff" : "debuff", character.charClass.name);
                    MasterLogger.Singleton.RpcLogMessage(message);
                }
            }
        }
    }

    [Server]
    private void RemoveBuffFromCharacters(RuntimeBuff buff, List<int> removeFromCharacters)
    {
        IAppliablBuffDataSO appliedBuff = buff.Data as IAppliablBuffDataSO;
        if(appliedBuff != null)
            appliedBuff.UnApply(removeFromCharacters);

        foreach(int affectedCharacterID in removeFromCharacters.ToArray())
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.RemoveAffectingBuff(buff);
            buff.AffectedCharacterIDs.Remove(affectedCharacterID);
        }

        Sprite buffIcon = buff.Data.Icon;
        if (buffIcon != null)            
            this.RpcRemoveBuffIconFromCharacters(buff.UniqueID, removeFromCharacters);

        ITriggeredBuff triggeredBuff = buff.Data as ITriggeredBuff;
        if (triggeredBuff != null)
        {
            triggeredBuff.RemoveTriggerListenersForBuff(buff, removeFromCharacters);
        }

        IConditionalBuff conditionalBuff = buff.Data as IConditionalBuff;
        if (conditionalBuff != null)
        {
            conditionalBuff.RemoveConditionListenersForBuff(buff, removeFromCharacters);
        }

        bool removingFromAllAffected = (removeFromCharacters == buff.AffectedCharacterIDs);
        RuntimeBuffAbility abilityBuff = buff.GetComponent<RuntimeBuffAbility>();
        if (abilityBuff != null && removingFromAllAffected)
            GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID].RemoveOwnedBuff(buff);
    }

    [Server]
    internal void RemoveConditionalBuffFromCharacter(int triggeredCharacterID, RuntimeBuff buff)
    {
        this.RemoveBuffFromCharacters(buff, new List<int>() { triggeredCharacterID });
    }

    [Server]
    internal void RemoveBuffsOnDeath(PlayerCharacter character)
    {
        foreach (RuntimeBuff buff in character.affectedByBuffs.ToArray())
        {
            if (buff.Data.DurationType == DurationType.eternal)
            {
                continue;
            }

            this.RemoveBuffFromCharacters(buff, new List<int> { character.CharClassID });
        }
    }

    [ClientRpc]
    private void RpcAddBuffIcons(int buffID, List<int> affectedCharacterIDs, string buffDataID, int remainingDuration)
    {
        TurnOrderHUD.Singleton.AddBuffIcons(buffID, affectedCharacterIDs, buffDataID, remainingDuration);
    }

    [ClientRpc]
    private void RpcUpdateBuffIconDurations(int buffID, List<int> affectedCharacterIDs, string buffDataID, int remainingDuration)
    {
        TurnOrderHUD.Singleton.UpdateBuffIconDurations(buffID, affectedCharacterIDs, buffDataID, remainingDuration);
    }

    [ClientRpc]
    private void RpcRemoveBuffIconFromCharacters(int buffID, List<int> affectedCharacterIDs)
    {
        TurnOrderHUD.Singleton.RemoveBuffIconFromCharacters(buffID, affectedCharacterIDs);
    }
}