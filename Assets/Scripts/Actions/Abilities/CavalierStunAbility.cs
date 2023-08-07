﻿using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CavalierStunAbility : IAbilityAction, ITargetedAction, IBuffSource
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

    public Type AppliesBuffType { get => typeof(CavalierStunEffect); }

    [Server]
    public void ServerUse()
    {
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        ActionExecutor.Singleton.AbilityAttack(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);
        
        //ability was used only as attack on obstacle
        if (!TargetHex.HoldsACharacter() || TargetHex.GetHeldCharacterObject() == null)
            return;

        Debug.Log("Using cavalier stun debuff!");
        List<int> affectedCharacterIDs = new List<int> { this.TargetHex.holdsCharacterWithClassID };
        IBuffEffect buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffType, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);
        BuffManager.Singleton.ApplyNewBuff(buff);

    }

    [Server]
    public bool ServerValidate()
    {

        //TODO check for individual ability uses instead of single hasUsedAbility to allow multiple abilities

        if (this.ActorCharacter != null &&
            this.ActorHex != null &&
            this.TargetHex != null &&
            this.RequestingPlayerID != -1 &&
            this.ActorHex.HoldsACharacter() &&
            this.ActorHex.GetHeldCharacterObject() == this.ActorCharacter &&
            this.RequestingPlayerID == this.ActorCharacter.OwnerID &&
            GameController.Singleton.ItsThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(this.ActorCharacter.CharClassID) &&
            ITargetedAction.ValidateTarget(this) &&
            !this.ActorCharacter.AbilityUsesPerRoundExpended(this.AbilityStats.stringID) &&
            !this.ActorCharacter.AbilityOnCooldown(this.AbilityStats.stringID)
            )
            return true;
        else
            return false;
    }
}