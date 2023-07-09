using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ActionExecutor : NetworkBehaviour
{
    public static ActionExecutor instance;

    public void Awake()
    {
        ActionExecutor.instance = this;
    }

    [Server]
    public bool TryAttackCharacter(int actingPlayerID, PlayerCharacter attackerCharacter, PlayerCharacter defenderCharacter, CharacterStats attackerStats, CharacterStats defenderStats, Hex attackerHex, Hex defenderHex)
    {
        IAction toTry = ActionFactory.CreateAttackAction(actingPlayerID, attackerCharacter, defenderCharacter, attackerStats, defenderStats, attackerHex, defenderHex);
        return this.UseAction(toTry);
    }

    [Server]
    public bool TryAttackObstacle(int actingPlayerID, PlayerCharacter attackerCharacter, CharacterStats attackerStats, Hex attackerHex, Hex defenderHex)
    {
        IAction toTry = ActionFactory.CreateAttackAction(actingPlayerID, attackerCharacter, attackerStats, attackerHex, defenderHex);
        return this.UseAction(toTry);
    }

    [Server]
    public bool TryAbility(int actingPlayerID, PlayerCharacter actingCharacter, CharacterAbility ability, Hex source, Hex target)
    {
        IAbilityAction toTry = ActionFactory.CreateAbilityAction(actingPlayerID, actingCharacter, ability, source, target);
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
