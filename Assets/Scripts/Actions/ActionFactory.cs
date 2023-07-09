using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionFactory : MonoBehaviour
{
    public static IAttackAction CreateAttackAction(int requestingPlayerID,
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

        //ITargetedAction
        attackAction.TargetHex = defenderHex;

        //IAttackAction
        attackAction.AttackerStats = attackerStats;
        attackAction.DefenderCharacter = defenderCharacter;
        attackAction.DefenderStats = defenderStats;

        return attackAction;
    }

    public static IAbilityAction CreateAbilityAction(int requestingPlayerID, PlayerCharacter actingCharacter, CharacterAbility ability, Hex userHex, Hex targetHex)
    {
        IAbilityAction abilityAction = (IAbilityAction) Activator.CreateInstance(ability.abilityActionType);

        //IAction
        abilityAction.RequestingPlayerID = requestingPlayerID;
        abilityAction.ActorCharacter = actingCharacter;
        abilityAction.ActorHex = userHex;

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
