using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PaladinTeamBuffAbility : IAbilityAction, IBuffSource, ITargetedAction, ICooldownedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    //IBuffSource
    public Type AppliesBuffType { get => typeof(PaladinTeamBuffEffect); }
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    [Server]
    public void ServerUse()
    {        
        Debug.Log("Using Paladin team buff!");
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);
        List<int> affectedCharacterIDs = new();

        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            if(GameController.Singleton.HeOwnsThisCharacter(ActorCharacter.OwnerID, character.CharClassID) && character.CharClassID != ActorCharacter.CharClassID)
            {
                affectedCharacterIDs.Add(character.CharClassID);
            }
        }

        IBuffEffect buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffType, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);
        BuffManager.Singleton.ApplyNewBuff(buff);

    }

    [Server]
    public bool ServerValidate()
    {

        //TODO check for individual ability uses instead of single hasUsedAbility to allow multiple abilities

        if (this.ActorCharacter != null &&
            this.ActorHex != null &&
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