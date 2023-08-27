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
    internal IAbilityBuffEffect CreateAbilityBuff(Type buffType, CharacterAbilityStats abilityStats, int applyingCharacterID, List<int> affectedCharacterIDs)
    {        
        //IAbilityBuffEffect
        IAbilityBuffEffect buff = (IAbilityBuffEffect)Activator.CreateInstance(buffType);
        buff.AppliedByAbility = abilityStats;
        buff.ApplyingCharacterID = applyingCharacterID;

        //IBuffEffect
        buff.UniqueID = IDGenerator.GetNewID();
        buff.AffectedCharacterIDs = affectedCharacterIDs;

        //ITimedEffect
        ITimedEffect timedBuff = buff as ITimedEffect;
        if (timedBuff != null)
        {
            timedBuff.TurnDurationRemaining = abilityStats.buffTurnDuration + 1;
        }

        //IPermanentEffect => nothing to do for now...
        //IPassiveEffect => nothing to do for now...
        //TODO : IConditionalEffect setup some stuff here eventually

        return buff;
    }

    [Server]
    public void ApplyNewBuff(IBuff buff)
    {
        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.AddAffectingBuff(buff);

            string message = string.Format("{0} applied to {2}", buff.UIName, affectedCharacter.charClass.name);
            MasterLogger.Singleton.RpcLogMessage(message);
        }

        IAbilityBuffEffect abilityBuff = buff as IAbilityBuffEffect;
        if (abilityBuff != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID];
            applyingCharacter.AddOwnedBuff(abilityBuff);
        }

        IAppliedBuff appliedBuff = buff as IAppliedBuff;
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
            foreach(IAbilityBuffEffect abilityBuff in character.ownerOfBuffs.ToArray())
            {
                ITimedEffect timedBuff = abilityBuff as ITimedEffect;
                if (timedBuff == null)
                    continue;                
                timedBuff.TurnDurationRemaining--;
                if (timedBuff.TurnDurationRemaining == 0)
                {
                    this.RemoveBuff(abilityBuff);
                }
            }

            foreach (IBuff buff in character.affectedByBuffs.ToArray())
            {
                IAppliedBuff appliedBuff = buff as IAppliedBuff;

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
    private void RemoveBuff(IBuff buff)
    {
        IAppliedBuff appliedBuff = buff as IAppliedBuff;
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

        IAbilityBuffEffect abilityBuff = buff as IAbilityBuffEffect;
        if (abilityBuff != null)
            GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID].RemoveOwnedBuff(abilityBuff);
    }

    [Server]
    internal void RemoveRoundBuffsAppliedToCharacter(PlayerCharacter character)
    {
        foreach (IAppliedBuff buff in character.affectedByBuffs.ToArray())
        {
            IPassiveEffect passiveBuff = buff as IPassiveEffect;
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