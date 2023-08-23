using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PriestResurrectAbility : IAbilityAction, ITargetedAction
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
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} using {1}", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);
        
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);
        PlayerCharacter toResurrect = this.TargetHex.GetHeldCorpseCharacterObject();
        int lifeOnResurrection = toResurrect.CurrentStats.maxHealth / 2;
        toResurrect.Resurrect(lifeOnResurrection);
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