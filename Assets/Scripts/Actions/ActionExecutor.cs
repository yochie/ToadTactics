using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class ActionExecutor : NetworkBehaviour
{
    public AttackEvent attackEvent;
    public MoveEvent moveEvent;
    public AbilityEvent abilityEvent;

    //TODO : remove this field, should simply be referenced by mapinputhandler
    public static ActionExecutor Singleton { get; private set; }

    public void Awake()
    {
        ActionExecutor.Singleton = this;
    }

    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter movingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];

        bool success = ActionExecutor.Singleton.TryMove(sender, playerID, movingCharacter, movingCharacter.currentStats, source, dest);
        if (success)
            this.CheckForRoundEnd();
    }


    [Command(requiresAuthority = false)]
    public void CmdAttack(Hex source, Hex target, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter attackingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
        PlayerCharacter targetedCharacter = null;
        if (target.HoldsACharacter())
        {
            targetedCharacter = GameController.Singleton.playerCharacters[target.holdsCharacterWithClassID];
        }

        bool success;
        if (targetedCharacter != null)
        {
            success = ActionExecutor.Singleton.TryAttackCharacter(sender, playerID, attackingCharacter, targetedCharacter, attackingCharacter.currentStats, targetedCharacter.currentStats, source, target);
        }
        else
        {
            success = ActionExecutor.Singleton.TryAttackObstacle(sender, playerID, attackingCharacter, attackingCharacter.currentStats, source, target);
        }
        if (success)
            this.CheckForRoundEnd();
    }

    [Command(requiresAuthority = false)]
    internal void CmdUseAbility(Hex source, Hex target, CharacterAbilityStats abilityStats, NetworkConnectionToClient sender = null)
    {
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        PlayerCharacter usingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];

        bool success = ActionExecutor.Singleton.TryAbility(sender, playerID, usingCharacter, abilityStats, source, target);
        if (success)
            this.CheckForRoundEnd();
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

        bool moveSuccess = this.TryAction(toTry);
        return moveSuccess;
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
        IAttackAction attackAction = ActionFactory.CreateAttackAction(sender, actingPlayerID, attackerCharacter, defenderCharacter, attackerStats, defenderStats, attackerHex, defenderHex);
        bool success = this.TryAction(attackAction);
        if (success)
            defenderCharacter.RpcOnCharacterLifeChanged(defenderCharacter.CurrentLife(), defenderCharacter.currentStats.maxHealth);
        return success;
    }

    [Server]
    public bool TryAttackObstacle(NetworkConnectionToClient sender,
                                  int actingPlayerID,
                                  PlayerCharacter attackerCharacter,
                                  CharacterStats attackerStats,
                                  Hex attackerHex,
                                  Hex defenderHex)
    {
        IAttackAction attackAction = ActionFactory.CreateObstacleAttackAction(sender, actingPlayerID, attackerCharacter, attackerStats, attackerHex, defenderHex);
        bool success = this.TryAction(attackAction);
        return success;
    }

    [Server]
    public bool TryAbility(NetworkConnectionToClient sender,
                           int actingPlayerID,
                           PlayerCharacter actingCharacter,
                           CharacterAbilityStats ability,
                           Hex source,
                           Hex target)
    {
        IAbilityAction toTry = ActionFactory.CreateAbilityAction(sender, actingPlayerID, actingCharacter, ability, source, target);
        return this.TryAction(toTry);
    }

    [Server]
    private bool TryAction(IAction action)
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

    [Server]
    private void CheckForRoundEnd()
    {
        foreach(PlayerCharacter p in GameController.Singleton.playerCharacters.Values)
        {
            if (p.isKing && p.IsDead())
            {
                Debug.Log("The king is dead. Long live the king.");
                GameController.Singleton.EndRound(p.ownerID);
            }
        }
    }


    //Utility for validation of ITargetedActions and to find attack range in MapPathFinder
    public static bool IsValidTargetType(PlayerCharacter actor, Hex targetedHex, List<TargetType> allowedTargets)
    {
        bool selfTarget = false;
        bool friendlyTarget = false;
        bool ennemyTarget = false;
        if (targetedHex.HoldsACharacter() || targetedHex.HoldsACorpse())
        {
            PlayerCharacter targetedCharacter = targetedHex.HoldsACharacter() ? targetedHex.GetHeldCharacterObject() : targetedHex.GetHeldCorpseCharacterObject();
            selfTarget = (actor.charClassID == targetedCharacter.charClassID);
            friendlyTarget = (actor.ownerID == targetedCharacter.ownerID);
            ennemyTarget = !friendlyTarget;
        }
        bool liveTarget = targetedHex.HoldsACharacter();
        bool corpseTarget = targetedHex.HoldsACorpse();
        bool emptyTarget = !targetedHex.HoldsACharacter() && !targetedHex.HoldsACorpse() && (targetedHex.holdsObstacle == ObstacleType.none);
        bool obstacleTarget = targetedHex.holdsObstacle != ObstacleType.none;

        if (!allowedTargets.Contains(TargetType.ennemy_chars) && (ennemyTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.other_friendly_chars) && (friendlyTarget && !selfTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.self) && (selfTarget && liveTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.empty_hex) && emptyTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.obstacle) && obstacleTarget)
            return false;

        if (!allowedTargets.Contains(TargetType.ennemy_corpse) && (ennemyTarget && corpseTarget))
            return false;

        if (!allowedTargets.Contains(TargetType.friendly_corpse) && (friendlyTarget && corpseTarget))
            return false;

        return true;
    }
}
