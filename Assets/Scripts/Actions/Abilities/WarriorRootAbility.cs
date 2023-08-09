using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WarriorRootAbility : IAbilityAction, ITargetedAction, IBuffSource
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    public Type AppliesBuffType { get => typeof(WarriorRootEffect); }

    [Server]
    public void ServerUse()
    {
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        Debug.Log("Using warrior root debuff!");
        List<Hex> hexesInAOE = MapPathfinder.RangeIgnoringObstacles(this.TargetHex, this.AbilityStats.aoe, Map.Singleton.hexGrid);
        List<int> targetsIDs = new();
        foreach (Hex hex in hexesInAOE)
        {
            if (hex.HoldsACharacter() && hex.GetHeldCharacterObject().OwnerID != this.RequestingPlayerID)
                targetsIDs.Add(hex.GetHeldCharacterObject().CharClassID);
        }        
        IBuffEffect buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffType, this.AbilityStats, this.ActorCharacter.CharClassID, targetsIDs);
        BuffManager.Singleton.ApplyNewBuff(buff);

    }

    [Server]
    public bool ServerValidate()
    {

        //TODO check for individual ability uses instead of single hasUsedAbility to allow multiple abilities

        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this) &&
            IAbilityAction.ValidateCooldowns(this)
            )
            return true;
        else
            return false;
    }
}