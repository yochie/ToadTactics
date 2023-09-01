using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WarriorRootAbility : IAbilityAction, ITargetedAction, IActivatedBuffSource
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

    //IAreaTargeter
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }
    
    //IBuffSource
    public IBuffDataSO AppliesBuffOnActivation { get => BuffDataSO.Singleton.GetBuffData("WarriorRootBuff"); }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} using {1}", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);

        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);
        List<int> affectedCharacters = targetedHexes
            .Where(hex => hex.HoldsACharacter() && hex.GetHeldCharacterObject().OwnerID != this.RequestingPlayerID)
            .Select(hex => hex.holdsCharacterWithClassID).ToList();
        RuntimeBuff buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffOnActivation, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacters);
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