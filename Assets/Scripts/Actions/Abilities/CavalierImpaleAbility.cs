using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CavalierImpaleAbility : IAbilityAction, ITargetedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats Ability { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    [Server]
    public void ServerUse()
    {
        //Action attack = new CustomAttackAction(user, target, abilityStats.damage, abilityStats.damageType, damageIterations: 1);
        //if (!attack.validate){ Debug.Log(); return;}
        //attack.CmdUse();

        //target.addBuff(new StunBuff(user, 1));
        //throw new System.NotImplementedException();
        Debug.Log("Using cavalier ability!");
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
            this.RequestingPlayerID == this.ActorCharacter.ownerID &&
            GameController.Singleton.ItsThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(this.ActorCharacter.charClassID) &&
            ITargetedAction.ValidateTarget(this) &&
            !this.ActorCharacter.hasUsedAbility
            )
            return true;
        else
            return false;
    }
}