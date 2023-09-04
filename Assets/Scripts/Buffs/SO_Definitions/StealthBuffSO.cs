using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(fileName = "StealthBuff", menuName = "Buffs/StealthBuff")]

public class StealthBuffSO : ScriptableObject, IConditionalBuff, IStealthModifier, IMovementModifier, IAppliablBuffDataSO
{
    [field: SerializeField]
    public string stringID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField]
    public bool IsPositive { get; set; }

    [field: SerializeField]
    public DurationType DurationType { get; set; }

    [field: SerializeField]
    public int TurnDuration { get; set; }

    [field: SerializeField]
    public Sprite Icon { get; set; }

    [field: SerializeField]
    public string InlineConditionDescription { get; set; }

    [field: SerializeField]
    public IntGameEventSO HitEvent { get; set; }

    [field: SerializeField]
    public IntIntGameEventSO AttackEvent { get; set; }

    [field: SerializeField]
    public int StealthLayersOffset { get; set; }

    [field: SerializeField]
    public int MovementOffset { get; set; }

    [field: SerializeField]
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        return this.GetStatModificationsDictionnary();
    }

    public string GetTooltipDescription()
    {
        return "Untargetable and increased movement. Lost on damage taken or attack.";
    }

    [Server]
    public void OnEndEvent(PlayerCharacter triggeredCharacter, RuntimeBuff buff)
    {
        //Debug.Log("Conditional buff end event triggered");

        BuffManager.Singleton.RemoveConditionalBuffFromCharacter(triggeredCharacter, buff);
    }

    [Server]
    public void SetupConditionListeners(RuntimeBuff buff)
    {
        foreach(int characterID in buff.AffectedCharacterIDs)
        {
            PlayerCharacter listeningCharacter = GameController.Singleton.PlayerCharactersByID[characterID];

            //Hit listener
            IntGameEventSOListener hitListener = listeningCharacter.gameObject.AddComponent(typeof(IntGameEventSOListener)) as IntGameEventSOListener;
            listeningCharacter.AddListenerForBuff(buff, hitListener);
            hitListener.Event = this.HitEvent;
            hitListener.RegisterManually();
            hitListener.Response.AddListener(
                (int hitCharacterID) => { 
                    if(hitCharacterID == listeningCharacter.CharClassID)
                        this.OnEndEvent(listeningCharacter, buff); 
                });

            //Attack listener
            IntIntGameEventSOListener attackListener = listeningCharacter.gameObject.AddComponent(typeof(IntIntGameEventSOListener)) as IntIntGameEventSOListener;
            listeningCharacter.AddListenerForBuff(buff, attackListener);
            attackListener.Event = this.AttackEvent;
            attackListener.RegisterManually();
            attackListener.Response.AddListener(
                (int attackingCharacter, int defenderCharacter) => {
                    if (attackingCharacter == listeningCharacter.CharClassID)
                        this.OnEndEvent(listeningCharacter, buff);
                });
        }
    }


    [Server]
    public void RemoveConditionListenersForBuff(RuntimeBuff runtimeBuff, List<int> removeFromCharacters) 
    {
        foreach (int characterID in removeFromCharacters)
        {
            PlayerCharacter triggeredCharacter = GameController.Singleton.PlayerCharactersByID[characterID];
            triggeredCharacter.RemoveListenersForBuff(runtimeBuff);
        }
    }

    public Dictionary<string, string> GetStatModificationsDictionnary()
    {
        Dictionary<string, string>  stats = new();
        stats.Add("Stealthy", "yes");
        stats.Add("Movement", string.Format("+{0}", this.MovementOffset));
        return stats;
    }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        CharacterStats newStats = new CharacterStats(playerCharacter.CurrentStats, 
            moveSpeed: playerCharacter.CurrentStats.moveSpeed + this.MovementOffset,
            stealthLayers: playerCharacter.CurrentStats.stealthLayers + this.StealthLayersOffset);
        playerCharacter.SetCurrentStats(newStats);
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        CharacterStats newStats = new CharacterStats(playerCharacter.CurrentStats,
            moveSpeed: playerCharacter.CurrentStats.moveSpeed - this.MovementOffset,
            stealthLayers: playerCharacter.CurrentStats.stealthLayers - this.StealthLayersOffset);
        playerCharacter.SetCurrentStats(newStats);
    }

    public void Apply(List<int> applyToCharacterIDs, bool isReapplication)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.ApplyStatModification(affectedCharacter);
        }
    }

    public void UnApply(List<int> unApplyFromCharacterIDs)
    {
        foreach (int affectedCharacterID in unApplyFromCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.RemoveStatModification(affectedCharacter);
        }
    }
}
