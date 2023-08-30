using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    private void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
    }

    [Server]
    internal RuntimeBuff CreateAbilityBuff(IBuffDataSO buffData, CharacterAbilityStats abilityStats, int applyingCharacterID, List<int> affectedCharacterIDs)
    {        
        RuntimeBuff runtimeBuff = new RuntimeBuff();
        runtimeBuff.UniqueID = IDGenerator.GetNewID();
        runtimeBuff.AffectedCharacterIDs = affectedCharacterIDs;
        runtimeBuff.BuffData = buffData;

        AbilityRuntimeBuff abilityBuffComponent = new AbilityRuntimeBuff();
        abilityBuffComponent.AppliedByAbility = abilityStats;
        abilityBuffComponent.ApplyingCharacterID = applyingCharacterID;
        runtimeBuff.AddComponent(abilityBuffComponent);
        
        if(buffData.DurationType == DurationType.timed)
        {
            TimedRuntimeBuff timedBuffComponent = new TimedRuntimeBuff();
            timedBuffComponent.TurnDurationRemaining = abilityStats.buffTurnDuration + 1;
            runtimeBuff.AddComponent(timedBuffComponent);
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

            string message = string.Format("{0} applied to {1}", buff.UIName, affectedCharacter.charClass.name);
            MasterLogger.Singleton.RpcLogMessage(message);
        }

        AbilityRuntimeBuff abilityComponent = (AbilityRuntimeBuff) buff.GetComponent(typeof(AbilityRuntimeBuff));
        if (abilityComponent != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharactersByID[abilityComponent.ApplyingCharacterID];
            applyingCharacter.AddOwnedBuff(buff);
        }

        IAppliablBuff appliedBuff = buff as IAppliablBuff;
        if (appliedBuff != null)
        {
            appliedBuff.ApplyEffect(appliedBuff.AffectedCharacterIDs, isReapplication: false);
        }

        IDisplayedBuff displayedBuff = buff as IDisplayedBuff;
        if(displayedBuff != null)
        {
            this.RpcAddBuffIcons(displayedBuff.UniqueID, displayedBuff.AffectedCharacterIDs, displayedBuff.IconName);
        }
    }
    
    [Server]
    public void TickBuffsForTurn(int playingCharacterID)
    {        
        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            if (character.CharClassID != playingCharacterID)
                continue;
            foreach(AbilityRuntimeBuff abilityBuff in character.ownerOfBuffs.ToArray())
            {
                TimedRuntimeBuff timedBuff = abilityBuff as TimedRuntimeBuff;
                if (timedBuff == null)
                    continue;                
                timedBuff.TurnDurationRemaining--;
                if (timedBuff.TurnDurationRemaining == 0)
                {
                    this.RemoveBuff(abilityBuff);
                }
            }

            foreach (IBuffDataSO buff in character.affectedByBuffs.ToArray())
            {
                IAppliablBuff appliedBuff = buff as IAppliablBuff;

                if (appliedBuff != null && appliedBuff.NeedsToBeReAppliedEachTurn && !character.IsDead)
                {
                    appliedBuff.ApplyEffect(new List<int> { character.CharClassID }, isReapplication: true);
                    string message = string.Format("Ticking {0} {1} on {2}", appliedBuff.UIName, appliedBuff.IsPositive ? "buff" : "debuff", character.charClass.name);
                    MasterLogger.Singleton.RpcLogMessage(message);
                }
            }
        }
    }

    [Server]
    private void RemoveBuff(IBuffDataSO buff)
    {
        IAppliablBuff appliedBuff = buff as IAppliablBuff;
        if(appliedBuff != null)
            appliedBuff.UnApply(buff.AffectedCharacterIDs);

        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.RemoveAffectingBuff(buff);
        }

        IDisplayedBuff displayedBuff = buff as IDisplayedBuff;
        if (displayedBuff != null)            
            this.RpcRemoveBuffIcons(displayedBuff.UniqueID, displayedBuff.AffectedCharacterIDs);

        AbilityRuntimeBuff abilityBuff = buff as AbilityRuntimeBuff;
        if (abilityBuff != null)
            GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID].RemoveOwnedBuff(abilityBuff);
    }

    [Server]
    internal void RemoveRoundBuffsAppliedToCharacter(PlayerCharacter character)
    {
        foreach (IAppliablBuff buff in character.affectedByBuffs.ToArray())
        {
            IPassiveAbility passiveBuff = buff as IPassiveAbility;
            if (passiveBuff != null)
            {
                continue;
            }
            buff.UnApply(new List<int> { character.CharClassID });
            buff.AffectedCharacterIDs.Remove(character.CharClassID);
            character.RemoveAffectingBuff(buff);
            this.RpcRemoveBuffIcons(buff.UniqueID, new List<int> { character.CharClassID });
        }
    }

    [ClientRpc]
    private void RpcAddBuffIcons(int buffID, List<int> affectedCharacterIDs, string iconName)
    {
        TurnOrderHUD.Singleton.AddBuffIcons(buffID, affectedCharacterIDs, iconName);
    }

    [ClientRpc]
    private void RpcRemoveBuffIcons(int buffID, List<int> affectedCharacterIDs)
    {
        TurnOrderHUD.Singleton.RemoveBuffIcons(buffID, affectedCharacterIDs);
    }
}