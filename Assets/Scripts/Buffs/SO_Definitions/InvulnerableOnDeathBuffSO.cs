using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(fileName = "InvulnerableOnDeathBuff", menuName = "Buffs/InvulnerableOnDeathBuff")]

public class InvulnerableOnDeathBuffSO : ScriptableObject, ITriggeredBuff
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

    //Should be death event with classID as arg here
    [field: SerializeField]
    public IntGameEventSO DeathEvent { get; set; }

    [field: SerializeField]
    private ScriptableObject AppliesBuff { get; set; }

    [field: SerializeField]
    public int MaxTriggers { get; set; }

    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        var toReturn = new Dictionary<string, string>();
        toReturn.Add("Trigger", string.Format("death"));
        toReturn.Add("# triggers", string.Format("{0}/round",this.MaxTriggers.ToString()));
        return toReturn;
    }

    public string GetTooltipDescription()
    {
        return "On death, will grant temporary invulnerability instead.";
    }

    [Server]
    public void OnTrigger(int dyingCharacterID, PlayerCharacter triggeredCharacter, RuntimeBuffAbility sourceAbilityComponent, RuntimeBuffTriggerCounter sourceTriggerCounterComponent)
    {
        Debug.Log("Triggered buff");

        if (dyingCharacterID != triggeredCharacter.CharClassID)
            return;
        if (sourceAbilityComponent == null)
            throw new Exception("Barb invulnerability should have an ability buff as source.");
        if (sourceTriggerCounterComponent.RemainingTriggers > 0)
            sourceTriggerCounterComponent.RemainingTriggers--;
        else
            return;

        triggeredCharacter.Resurrect(1);

        IBuffDataSO triggeredBuffData = this.AppliesBuff as IBuffDataSO;
        if (triggeredBuffData == null)
            throw new Exception("Triggered buff in buff data does not implement IBuffDataSO as expected.");

        RuntimeBuff triggeredBuff = BuffManager.Singleton.CreateAbilityBuff(triggeredBuffData, sourceAbilityComponent.AppliedByAbility, sourceAbilityComponent.ApplyingCharacterID, new() { dyingCharacterID });
        RuntimeBuffTimeout timedComponent = triggeredBuff.GetComponent<RuntimeBuffTimeout>();
        if (timedComponent != null)
        {
            //Since buff durations are usually increased to ignore turn they are applied (because they are usually applied during by a character during his own turn)
            //we need to decrement remaining turns if its not currently applying characters turn to actually expire buff on next turn if its duration is 1
            if (!GameController.Singleton.ItsThisCharactersTurn(dyingCharacterID))
            {
                timedComponent.TurnDurationRemaining--;
            }
        }
        BuffManager.Singleton.ApplyNewBuff(triggeredBuff);
    }

    [Server]
    public void SetupTriggerListeners(RuntimeBuff sourceBuff)
    {
        foreach(int characterID in sourceBuff.AffectedCharacterIDs)
        {
            PlayerCharacter triggeredCharacter = GameController.Singleton.PlayerCharactersByID[characterID];
            
            IntGameEventSOListener listener = triggeredCharacter.gameObject.AddComponent(typeof(IntGameEventSOListener)) as IntGameEventSOListener;
            triggeredCharacter.AddListenerForBuff(sourceBuff, listener);
            listener.Event = this.DeathEvent;
            listener.RegisterManually();
            RuntimeBuffAbility sourceAbilityComponent = sourceBuff.GetComponent<RuntimeBuffAbility>();
            RuntimeBuffTriggerCounter sourceTriggerCounterComponent = sourceBuff.GetComponent<RuntimeBuffTriggerCounter>();

            listener.Response.AddListener((int dyingCharacter) => { this.OnTrigger(dyingCharacter, triggeredCharacter, sourceAbilityComponent, sourceTriggerCounterComponent); });
        }             
    }

    [Server]
    public void RemoveTriggerListenersForBuff(RuntimeBuff runtimeBuff, List<int> removeFromCharacters) 
    {
        foreach (int characterID in removeFromCharacters)
        {
            PlayerCharacter triggeredCharacter = GameController.Singleton.PlayerCharactersByID[characterID];
            triggeredCharacter.RemoveListenersForBuff(runtimeBuff);
        }
    }
}
