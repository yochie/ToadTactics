using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

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

        //IAttackAction
        attackAction.AttackerStats = attackerStats;
        attackAction.DefenderCharacter = defenderCharacter;
        attackAction.DefenderStats = defenderStats;

        return attackAction;
    }

    //For attacking obstacles
    public static IAttackAction CreateAttackAction(NetworkConnectionToClient sender, 
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

        //IAttackAction
        attackAction.AttackerStats = attackerStats;
        attackAction.DefenderCharacter = null;        

        return attackAction;
    }

    public static IAbilityAction CreateAbilityAction(NetworkConnectionToClient sender, int requestingPlayerID, PlayerCharacter actingCharacter, CharacterAbility ability, Hex userHex, Hex targetHex)
    {
        IAbilityAction abilityAction = (IAbilityAction) Activator.CreateInstance(ability.abilityActionType);

        //IAction
        abilityAction.RequestingPlayerID = requestingPlayerID;
        abilityAction.ActorCharacter = actingCharacter;
        abilityAction.ActorHex = userHex;
        abilityAction.RequestingClient = sender;


        //IAbilityAction
        abilityAction.Ability = ability;
        
        //ITargetedAction (conditionally)
        //Assign target hex if ability implements ITargetedAction
        if (typeof(ITargetedAction).IsAssignableFrom(ability.abilityActionType))
        {
            ITargetedAction actionWithTarget = abilityAction as ITargetedAction;
            actionWithTarget.TargetHex = targetHex;
        }
        
        return abilityAction;
    }
}
