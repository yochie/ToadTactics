using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WizardFireballAbility : IAbilityAction, ITargetedAction
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
        Debug.Log(message);
        logger.RpcLogMessage(message);
        
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> hexesInAOE = MapPathfinder.RangeIgnoringObstacles(this.TargetHex, this.AbilityStats.aoe, Map.Singleton.hexGrid);
        foreach (Hex hex in hexesInAOE)
        {
            if (hex.HoldsACharacter() || hex.HoldsAnObstacle())
                ActionExecutor.Singleton.AbilityAttack(this.ActorHex, hex, this.AbilityStats, this.RequestingClient);
        }

        //version for non obstacle destruction
        //List<PlayerCharacter> allTargets = new();
        //PlayerCharacter mainTarget= this.TargetHex.GetHeldCorpseCharacterObject();
        //allTargets.Add(mainTarget);
        //List<Hex> hexesInAOE = MapPathfinder.RangeIgnoringObstacles(this.TargetHex, this.AbilityStats.aoe, Map.Singleton.hexGrid);
        //foreach(Hex hex in hexesInAOE)
        //{
        //    if (!hex.HoldsACharacter())
        //        continue;

        //    PlayerCharacter characterInAOE = hex.GetHeldCharacterObject();
        //    allTargets.Add(characterInAOE);
        //}

        //foreach(PlayerCharacter character in allTargets)
        //{
        //    ActionExecutor.Singleton.AbilityAttack(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);
        //}
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