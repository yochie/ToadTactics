using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ActionExecutor : NetworkBehaviour
{
    //TODO : remove this field, should simply be referenced by mapinputhandler
    public static ActionExecutor Singleton { get; private set; }

    public void Awake()
    {
        ActionExecutor.Singleton = this;
    }

    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        Debug.Log("Pikachu, move!");

        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter movingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];

        ActionExecutor.Singleton.TryMove(sender, playerID, movingCharacter, movingCharacter.currentStats, source, dest);
    }


    [Command(requiresAuthority = false)]
    public void CmdAttack(Hex source, Hex target, NetworkConnectionToClient sender = null)
    {
        Debug.Log("Pikachu, attack!");

        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter attackingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
        PlayerCharacter targetedCharacter = null;
        if (target.HoldsACharacter())
        {
            targetedCharacter = GameController.Singleton.playerCharacters[target.holdsCharacterWithClassID];
        }


        if (targetedCharacter != null)
        {
            ActionExecutor.Singleton.TryAttackCharacter(sender, playerID, attackingCharacter, targetedCharacter, attackingCharacter.currentStats, targetedCharacter.currentStats, source, target);

        }
        else
        {
            ActionExecutor.Singleton.TryAttackObstacle(sender, playerID, attackingCharacter, attackingCharacter.currentStats, source, target);
        }
    }

    [Server]
    public bool TryMove(NetworkConnectionToClient sender,
                                   int actingPlayerID,
                                   PlayerCharacter moverCharacter,                                   
                                   CharacterStats moverStats,                                   
                                   Hex moverHex,
                                   Hex targetHex)
    {
        IAction toTry = ActionFactory.CreateMoveAction(sender, actingPlayerID, moverCharacter, moverStats, moverHex, targetHex);
        return this.UseAction(toTry);
    }

    [Server]
    public bool TryAttackCharacter(NetworkConnectionToClient sender,
                                   int actingPlayerID,
                                   PlayerCharacter attackerCharacter,
                                   PlayerCharacter defenderCharacter,
                                   CharacterStats attackerStats,
                                   CharacterStats defenderStats,
                                   Hex attackerHex,
                                   Hex defenderHex)
    {
        IAction toTry = ActionFactory.CreateAttackAction(sender, actingPlayerID, attackerCharacter, defenderCharacter, attackerStats, defenderStats, attackerHex, defenderHex);
        return this.UseAction(toTry);
    }

    [Server]
    public bool TryAttackObstacle(NetworkConnectionToClient sender,
                                  int actingPlayerID,
                                  PlayerCharacter attackerCharacter,
                                  CharacterStats attackerStats,
                                  Hex attackerHex,
                                  Hex defenderHex)
    {
        IAction toTry = ActionFactory.CreateAttackAction(sender, actingPlayerID, attackerCharacter, attackerStats, attackerHex, defenderHex);
        return this.UseAction(toTry);
    }

    [Server]
    public bool TryAbility(NetworkConnectionToClient sender,
                           int actingPlayerID,
                           PlayerCharacter actingCharacter,
                           CharacterAbility ability,
                           Hex source,
                           Hex target)
    {
        IAbilityAction toTry = ActionFactory.CreateAbilityAction(sender, actingPlayerID, actingCharacter, ability, source, target);
        return this.UseAction(toTry);
    }

    [Server]
    private bool UseAction(IAction action)
    {
        if (action.ServerValidate())
        {
            action.ServerUse();
            return true;
        }
        else
        {
            Debug.LogFormat("Action {0} validation failed.", action);
            return false;
        }
    }


    //Utility for validation, used by individual IAction classes
    public static bool IsValidTargetType(PlayerCharacter actor, Hex targetedHex, List<TargetType> allowedTargets)
    {
        bool selfTarget = false;
        bool friendlyTarget = false;
        bool ennemyTarget = false;
        if (targetedHex.HoldsACharacter())
        {
            PlayerCharacter targetedCharacter = targetedHex.GetHeldCharacterObject();
            selfTarget = (actor.charClassID == targetedCharacter.charClassID);
            friendlyTarget = (actor.ownerID == targetedCharacter.ownerID);
            ennemyTarget = !friendlyTarget;

        }
        bool emptyTarget = !targetedHex.HoldsACharacter() && targetedHex.holdsObstacle == ObstacleType.none;
        bool obstacleTarget = targetedHex.holdsObstacle != ObstacleType.none;

        if (!allowedTargets.Contains(TargetType.ennemy_chars) && ennemyTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.other_friendly_chars) && friendlyTarget && !selfTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.self) && selfTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.empty_hex) && emptyTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.obstacle) && obstacleTarget)
            return false;

        return true;
    }
}
