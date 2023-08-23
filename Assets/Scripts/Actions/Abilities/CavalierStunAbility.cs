using Mirror;
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
    public void ServerUse(INetworkedLogger logger)
    {

        string message = string.Format("{0} using {1}", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);

        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        ActionExecutor.Singleton.AbilityAttack(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);
        
        //ability was used only as attack on obstacle or targeted character died
        if (!TargetHex.HoldsACharacter() || TargetHex.GetHeldCharacterObject() == null)
            return;

        List<int> affectedCharacterIDs = new List<int> { this.TargetHex.holdsCharacterWithClassID };
        IBuffEffect buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffType, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);
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