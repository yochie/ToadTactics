using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using System.Linq;

public class ActionFactory : MonoBehaviour
{
    public static IMoveAction CreateMoveAction(NetworkConnectionToClient sender,
                                           int requestingPlayerID,
                                           PlayerCharacter moverCharacter,
                                           CharacterStats moverStats,
                                           Hex moverHex,
                                           Hex targetHex)
    {
        IMoveAction moveAction = new DefaultMoveAction();

        //IAction
        moveAction.RequestingPlayerID = requestingPlayerID;
        moveAction.ActorCharacter = moverCharacter;
        moveAction.ActorHex = moverHex;
        moveAction.RequestingClient = sender;

        //ITargetedAction
        moveAction.TargetHex = targetHex;
        moveAction.AllowedTargetTypes = new List<TargetType> { TargetType.empty_hex };
        moveAction.RequiresLOS = false;
        moveAction.Range = moverStats.moveSpeed;

        //IMoveAction
        moveAction.MoverStats = moverStats;

        return moveAction;
    }

    //For attacking characters
    public static IAttackAction CreateAttackAction(NetworkConnectionToClient sender,
                                                   int requestingPlayerID,
                                                   PlayerCharacter attackerCharacter,
                                                   PlayerCharacter defenderCharacter,
                                                   CharacterStats attackerStats,
                                                   CharacterStats defenderStats,
                                                   Hex attackerHex,
                                                   Hex defenderHex)
    {
        IAttackAction attackAction = new DefaultAttackAction();

        //IAction
        attackAction.RequestingPlayerID = requestingPlayerID;
        attackAction.ActorCharacter = attackerCharacter;
        attackAction.ActorHex = attackerHex;
        attackAction.RequestingClient = sender;

        //ITargetedAction
        attackAction.TargetHex = defenderHex;
        attackAction.AllowedTargetTypes = attackerStats.allowedAttackTargets;
        attackAction.RequiresLOS = attackerStats.attacksRequireLOS;
        attackAction.Range = attackerStats.range;

        //IAttackAction
        attackAction.AttackerStats = attackerStats;
        attackAction.DefenderCharacter = defenderCharacter;
        attackAction.DefenderStats = defenderStats;

        return attackAction;
    }

    //For attacking obstacles
    public static IAttackAction CreateObstacleAttackAction(NetworkConnectionToClient sender, 
                                               int requestingPlayerID,
                                               PlayerCharacter attackerCharacter,
                                               CharacterStats attackerStats,
                                               Hex attackerHex,
                                               Hex defenderHex)
    {
        IAttackAction attackAction = new DefaultAttackAction();

        //IAction
        attackAction.RequestingPlayerID = requestingPlayerID;
        attackAction.ActorCharacter = attackerCharacter;
        attackAction.ActorHex = attackerHex;
        attackAction.RequestingClient = sender;

        //ITargetedAction
        attackAction.TargetHex = defenderHex;
        attackAction.AllowedTargetTypes = new List<TargetType> { TargetType.obstacle};
        attackAction.RequiresLOS = attackerStats.attacksRequireLOS;
        attackAction.Range = attackerStats.range;

        //IAttackAction
        attackAction.AttackerStats = attackerStats;
        attackAction.DefenderCharacter = null;

        return attackAction;
    }

    public static IAbilityAction CreateAbilityAction(NetworkConnectionToClient sender, int requestingPlayerID, PlayerCharacter actingCharacter, CharacterAbilityStats ability, Hex userHex, Hex targetHex)
    {
        if(ability.isPassive)
        {
            throw new Exception("Attempting to create action for passive ability. No can do.");
        }
        Type actionType = ClassDataSO.Singleton.GetActionTypeByID(ability.stringID);
        IAbilityAction abilityAction = (IAbilityAction) Activator.CreateInstance(actionType);

        //IAction
        abilityAction.RequestingPlayerID = requestingPlayerID;
        abilityAction.ActorCharacter = actingCharacter;
        abilityAction.ActorHex = userHex;
        abilityAction.RequestingClient = sender;

        //IAbilityAction
        abilityAction.AbilityStats = ability;
        
        //ITargetedAction (conditionally)
        //Assign target hex if ability implements ITargetedAction
        if (typeof(ITargetedAction).IsAssignableFrom(actionType))
        {
            ITargetedAction actionWithTarget = abilityAction as ITargetedAction;
            actionWithTarget.TargetHex = targetHex;
            actionWithTarget.AllowedTargetTypes = ability.allowedAbilityTargets;
            actionWithTarget.Range = ability.range;
            actionWithTarget.RequiresLOS = ability.requiresLOS;
        }
        
        return abilityAction;
    }

    public static AbilityAttackAction CreateAbilityAttackCharacterAction(NetworkConnectionToClient sender,
                                                                int requestingPlayerID,
                                                                PlayerCharacter attackerCharacter,
                                                                PlayerCharacter defenderCharacter,
                                                                CharacterStats attackerStats,
                                                                CharacterStats defenderStats,
                                                                CharacterAbilityStats abilityStats,
                                                                Hex attackerHex,
                                                                Hex defenderHex)
    {
        AbilityAttackAction abilityAttackAction = new AbilityAttackAction();

        //IAction
        abilityAttackAction.RequestingPlayerID = requestingPlayerID;
        abilityAttackAction.ActorCharacter = attackerCharacter;
        abilityAttackAction.ActorHex = attackerHex;
        abilityAttackAction.RequestingClient = sender;

        //ITargetedAction
        //We are trusting main ability to assign valid targets to these sub actions since we might want to use different criteria than those for main ability action
        abilityAttackAction.TargetHex = defenderHex;        
        abilityAttackAction.AllowedTargetTypes = Enum.GetValues(typeof(TargetType)).Cast<TargetType>().ToList();
        abilityAttackAction.RequiresLOS = false;
        abilityAttackAction.Range = 99;

        //IAttackAction
        abilityAttackAction.AttackerStats = attackerStats;
        abilityAttackAction.DefenderCharacter = defenderCharacter;
        abilityAttackAction.DefenderStats = defenderStats;

        //IAbilityAction
        abilityAttackAction.AbilityStats = abilityStats;

        return abilityAttackAction;
    }

    internal static AbilityAttackAction CreateAbilityAttackObstacleAction(NetworkConnectionToClient sender,
                                                                          int requestingPlayerID,
                                                                          PlayerCharacter attackerCharacter,
                                                                          CharacterStats attackerStats,
                                                                          CharacterAbilityStats abilityStats,
                                                                          Hex attackerHex,
                                                                          Hex defenderHex)
    {
        AbilityAttackAction abilityAttackAction = new AbilityAttackAction();

        //IAction
        abilityAttackAction.RequestingPlayerID = requestingPlayerID;
        abilityAttackAction.ActorCharacter = attackerCharacter;
        abilityAttackAction.ActorHex = attackerHex;
        abilityAttackAction.RequestingClient = sender;

        //ITargetedAction
        //We are trusting main ability to assign valid targets to these sub actions since we might want to use different criteria than those for main ability action
        abilityAttackAction.TargetHex = defenderHex;
        abilityAttackAction.AllowedTargetTypes = new List<TargetType> { TargetType.obstacle };
        abilityAttackAction.RequiresLOS = false;
        abilityAttackAction.Range = 99;

        //IAttackAction
        abilityAttackAction.AttackerStats = attackerStats;
        abilityAttackAction.DefenderCharacter = null;

        //IAbilityAction
        abilityAttackAction.AbilityStats = abilityStats;

        return abilityAttackAction;
    }
}
