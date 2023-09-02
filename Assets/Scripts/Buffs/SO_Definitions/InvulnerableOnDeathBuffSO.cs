using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InvulnerableOnDeathBuff", menuName = "Buffs/InvulnerableOnDeathBuff")]

public class InvulnerableOnDeathBuffSO : ScriptableObject, IIntEventTriggeredBuff
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
    public IntGameEventSO TriggerEvent { get; set; }

    [field: SerializeField]
    private ScriptableObject AppliesBuff { get; set; }


    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        return new();
    }

    public string GetTooltipDescription()
    {
        return "On death, will become invulnerable until next turn.";
    }

    public void OnTrigger(int dyingCharacterID, PlayerCharacter triggeredCharacter, RuntimeBuffAbility sourceAbilityComponent)
    {
        if (dyingCharacterID != triggeredCharacter.CharClassID)
            return;
        if (sourceAbilityComponent == null)
            throw new Exception("Barb invulnerability should have an ability buff as source.");

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

    public void SetupListeners(RuntimeBuff sourceBuff)
    {
        foreach(int characterID in sourceBuff.AffectedCharacterIDs)
        {
            PlayerCharacter triggeredCharacter = GameController.Singleton.PlayerCharactersByID[characterID];
            IntGameEventSOListener listener = triggeredCharacter.gameObject.AddComponent(typeof(IntGameEventSOListener)) as IntGameEventSOListener;
            listener.Event = this.TriggerEvent;
            listener.RegisterManually();
            RuntimeBuffAbility sourceAbilityComponent = sourceBuff.GetComponent<RuntimeBuffAbility>();
            listener.Response.AddListener((int dyingCharacter) => this.OnTrigger(dyingCharacter, triggeredCharacter, sourceAbilityComponent));
        }
             
    }
}
