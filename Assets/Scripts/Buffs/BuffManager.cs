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
        runtimeBuff.Data = buffData;

        RuntimeBuffAbility abilityBuffComponent = new RuntimeBuffAbility();
        abilityBuffComponent.AppliedByAbility = abilityStats;
        abilityBuffComponent.ApplyingCharacterID = applyingCharacterID;
        runtimeBuff.AddComponent(abilityBuffComponent);
        
        if(buffData.DurationType == DurationType.timed)
        {
            TimedRuntimeBuff timedBuffComponent = new TimedRuntimeBuff();
            timedBuffComponent.TurnDurationRemaining = buffData.TurnDuration + 1;
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

            string message = string.Format("{0} applied to {1}", buff.Data.UIName, affectedCharacter.charClass.name);
            MasterLogger.Singleton.RpcLogMessage(message);
        }

        RuntimeBuffAbility abilityComponent = buff.GetComponent<RuntimeBuffAbility>();
        if (abilityComponent != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharactersByID[abilityComponent.ApplyingCharacterID];
            applyingCharacter.AddOwnedBuff(buff);
        }

        IAppliablBuff appliedBuff = buff.Data as IAppliablBuff;
        if (appliedBuff != null)
        {
            appliedBuff.ApplyEffect(buff.AffectedCharacterIDs, isReapplication: false);
        }

        Sprite buffIcon = buff.Data.Icon;
        if(buffIcon != null)
        {
            this.RpcAddBuffIcons(buff.UniqueID, buff.AffectedCharacterIDs, buff.Data.stringID);
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
                TimedRuntimeBuff timedBuffComponent = ownedBuff.GetComponent<TimedRuntimeBuff>();
                if (timedBuffComponent == null)
                    continue;                
                timedBuffComponent.TurnDurationRemaining--;
                if (timedBuffComponent.TurnDurationRemaining == 0)
                {
                    this.RemoveBuff(ownedBuff);
                }
            }

            foreach (RuntimeBuff buff in character.affectedByBuffs.ToArray())
            {
                IAppliablBuff appliableBuff = buff.Data as IAppliablBuff;

                if (appliableBuff != null && appliableBuff.NeedsToBeReAppliedEachTurn && !character.IsDead)
                {
                    appliableBuff.ApplyEffect(new List<int> { character.CharClassID }, isReapplication: true);
                    string message = string.Format("Ticking {0} {1} on {2}", appliableBuff.UIName, appliableBuff.IsPositive ? "buff" : "debuff", character.charClass.name);
                    MasterLogger.Singleton.RpcLogMessage(message);
                }
            }
        }
    }

    [Server]
    private void RemoveBuff(RuntimeBuff buff)
    {
        IAppliablBuff appliedBuff = buff as IAppliablBuff;
        if(appliedBuff != null)
            appliedBuff.UnApply(buff.AffectedCharacterIDs);

        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.RemoveAffectingBuff(buff);
        }

        Sprite buffIcon = buff.Data.Icon;
        if (buffIcon != null)            
            this.RpcRemoveBuffIconFromCharacters(buff.UniqueID, buff.AffectedCharacterIDs);

        RuntimeBuffAbility abilityBuff = buff.GetComponent<RuntimeBuffAbility>();
        if (abilityBuff != null)
            GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID].RemoveOwnedBuff(buff);
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
            IAppliablBuff appliableBuff = buff.Data as IAppliablBuff;
            if (appliableBuff != null)
                appliableBuff.UnApply(new List<int> { character.CharClassID });
            buff.AffectedCharacterIDs.Remove(character.CharClassID);
            character.RemoveAffectingBuff(buff);
            if(buff.Data.Icon != null)
                this.RpcRemoveBuffIconFromCharacters(buff.UniqueID, new List<int> { character.CharClassID });
        }
    }

    [ClientRpc]
    private void RpcAddBuffIcons(int buffID, List<int> affectedCharacterIDs, string buffDataID)
    {
        TurnOrderHUD.Singleton.AddBuffIcons(buffID, affectedCharacterIDs, buffDataID);
    }

    [ClientRpc]
    private void RpcRemoveBuffIconFromCharacters(int buffID, List<int> affectedCharacterIDs)
    {
        TurnOrderHUD.Singleton.RemoveBuffIconFromCharacters(buffID, affectedCharacterIDs);
    }
}