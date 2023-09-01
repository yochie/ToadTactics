using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PaladinTeamBuffAbility : IAbilityAction, IActivatedBuffSource, ICooldownedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    //IBuffSource
    public IBuffDataSO AppliesBuffOnActivation { get => BuffDataSO.Singleton.GetBuffData("PaladinTeamDefensiveBuff"); }
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    //IAreaTargeter
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} using {1}", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);
        
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);

        List<int> affectedCharacterIDs = new();
        foreach(PlayerCharacter character in GameController.Singleton.PlayerCharactersByID.Values)
        {
            Hex characterPosition = Map.GetHex(Map.Singleton.hexGrid, Map.Singleton.characterPositions[character.CharClassID]);
            if(GameController.Singleton.HeOwnsThisCharacter(ActorCharacter.OwnerID, character.CharClassID) && !character.IsDead && targetedHexes.Contains(characterPosition))
            {
                affectedCharacterIDs.Add(character.CharClassID);
            }
        }

        RuntimeBuff buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffOnActivation, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);

        BuffManager.Singleton.ApplyNewBuff(buff);
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