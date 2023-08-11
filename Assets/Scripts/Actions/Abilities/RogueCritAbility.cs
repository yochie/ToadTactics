using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RogueCritAbility : IAbilityAction, ITargetedAction
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

    [Server]
    public void ServerUse()
    {
        Debug.Log("Using rogue crit!");
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);
        ActionExecutor.Singleton.AbilityAttack(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);
    }

    [Server]
    public bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this) &&
            IAbilityAction.ValidateCooldowns(this)
            )
            return true;
        else
            return false;
    }
}