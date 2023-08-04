using System;
using System.Collections.Generic;
using UnityEngine;

internal class BuffManager : MonoBehaviour
{
    public static BuffManager Singleton { get; private set; }

    private Dictionary<IBuffEffect, int> persistingPermanentBuffs;

    private void Awake()
    {
        if (BuffManager.Singleton != null)
            Destroy(BuffManager.Singleton.gameObject);
        BuffManager.Singleton = this;

        this.persistingPermanentBuffs = new();
        DontDestroyOnLoad(this.gameObject);
    }

    internal IAbilityBuffEffect CreateAbilityBuff(Type buffType, CharacterAbilityStats abilityStats, int applyingCharacterID, int targetCharacterID)
    {
        
        //IAbilityBuffEffect
        IAbilityBuffEffect buff = (IAbilityBuffEffect)Activator.CreateInstance(buffType);
        buff.AppliedByAbility = abilityStats;
        buff.ApplyingCharacterID = applyingCharacterID;

        //IBuffEffect
        buff.AffectedCharacterID = targetCharacterID;

        //ITimedEffect
        ITimedEffect timedBuff = buff as ITimedEffect;
        if (timedBuff != null)
        {
            timedBuff.TurnDurationRemaining = abilityStats.buffTurnDuration;
        }

        //IPermanentEffect
        IPermanentEffect permanentBuff = buff as IPermanentEffect;
        if(permanentBuff != null)
        {
            if (permanentBuff.PersistsBetweenRounds && !this.persistingPermanentBuffs.ContainsKey(buff))
                this.persistingPermanentBuffs.Add(buff, targetCharacterID);
        }

        //TODO : IConditionalEffect

        return buff;

    }
    public void ApplyBuff(IBuffEffect buff)
    {
        PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharacters[buff.AffectedCharacterID];
        affectedCharacter.AddAffectingBuff(buff);

        IAbilityBuffEffect abilityBuff = buff as IAbilityBuffEffect;
        if (abilityBuff != null)
        {
            PlayerCharacter applyingCharacter = GameController.Singleton.PlayerCharacters[abilityBuff.ApplyingCharacterID];
            applyingCharacter.AddAppliedBuff(buff);
        }

        buff.ApplyEffect(isReapplication: false);
    }
}