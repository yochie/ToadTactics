﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

internal class BuffManager : NetworkBehaviour
{
    public static BuffManager Singleton { get; private set; }

    private Dictionary<IBuffEffect, List<int>> persistingPermanentBuffs;

    private void Awake()
    {
        Debug.Log("BuffManager awoken");
        BuffManager.Singleton = this;

        this.persistingPermanentBuffs = new();
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
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

        //IPermanentEffect
        IPermanentEffect permanentBuff = buff as IPermanentEffect;
        if(permanentBuff != null)
        {
            if (permanentBuff.PersistsBetweenRounds && !this.persistingPermanentBuffs.ContainsKey(buff))
                this.persistingPermanentBuffs.Add(buff, affectedCharacterIDs);
        }

        //TODO : IConditionalEffect

        return buff;

    }
    [Server]
    public void ApplyNewBuff(IBuffEffect buff)
    {
        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.AddAffectingBuff(buff);
        }

        IAbilityBuffEffect abilityBuff = buff as IAbilityBuffEffect;
        if (abilityBuff != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharactersByID[abilityBuff.ApplyingCharacterID];
            applyingCharacter.AddAppliedBuff(buff);
        }

        buff.ApplyEffect(buff.AffectedCharacterIDs, isReapplication: false);

        this.RpcAddBuffIcons( buff.UniqueID, buff.AffectedCharacterIDs, buff.IconName);
    }

    [Server]
    public void TickBuffsForTurn(int playingCharacterID)
    {        
        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            if (character.CharClassID != playingCharacterID)
                continue;
            foreach(IBuffEffect buff in character.appliedBuffs.ToArray())
            {
                ITimedEffect timedBuff = buff as ITimedEffect;
                if (timedBuff == null)
                    continue;                
                timedBuff.TurnDurationRemaining--;
                if (timedBuff.TurnDurationRemaining == 0)
                {
                    this.RemoveBuff(buff, character);
                }
            }

            foreach (IBuffEffect buff in character.affectingBuffs.ToArray())
            {
                if (buff.NeedsToBeReAppliedEachTurn)
                {
                    buff.ApplyEffect(new List<int> { character.CharClassID }, isReapplication: true);
                }
            }
        }
    }

    [Server]
    private void RemoveBuff(IBuffEffect buff, PlayerCharacter appliedByCharacter = null)
    {
        buff.UnApply(buff.AffectedCharacterIDs);

        foreach(int affectedCharacterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            affectedCharacter.RemoveAffectingBuff(buff);
        }
        this.RpcRemoveBuffIcons(buff.UniqueID, buff.AffectedCharacterIDs);

        if (appliedByCharacter != null)
            appliedByCharacter.RemoveAppliedBuff(buff);
    }

    //TODO : call and fill
    [Server]
    public void ApplyPersistentBuffsForNewRound()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal void RemoveRoundBuffsAppliedToCharacter(PlayerCharacter character)
    {
        foreach (IBuffEffect buff in character.affectingBuffs.ToArray())
        {
            IPermanentEffect permanentBuff = buff as IPermanentEffect;
            if (permanentBuff != null && permanentBuff.PersistsBetweenRounds)
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
        TurnOrderHUD.Singleton.AddBuffIcons(buffID, affectedCharacterIDs, BuffIconsDataSO.Singleton.GetBuffIcon(iconName));
    }

    [ClientRpc]
    private void RpcRemoveBuffIcons(int buffID, List<int> affectedCharacterIDs)
    {
        TurnOrderHUD.Singleton.RemoveBuffIcons(buffID, affectedCharacterIDs);
    }


}