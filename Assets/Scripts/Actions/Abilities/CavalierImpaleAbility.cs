using Mirror;

public class CavalierImpaleAbility : NetworkBehaviour, IAbilityAction, ITargetedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbility Ability { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }

    [Server]
    public void ServerUse()
    {
        //Action attack = new CustomAttackAction(user, target, abilityStats.damage, abilityStats.damageType, damageIterations: 1);
        //if (!attack.validate){ Debug.Log(); return;}
        //attack.CmdUse();

        //target.addBuff(new StunBuff(user, 1));
        throw new System.NotImplementedException();
    }

    [Server]
    public bool ServerValidate()
    {
        throw new System.NotImplementedException();
    }
}