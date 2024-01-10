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
        Type moveActionType = ClassDataSO.Singleton.GetMoveActionTypeByID(moverCharacter.charClass.moveActionID);
        IMoveAction moveAction = (IMoveAction) Activator.CreateInstance(moveActionType);

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

    internal static CustomMoveAction CreateCustomMoveAction(NetworkConnectionToClient sender, int requestingPlayerID, PlayerCharacter moverCharacter, Hex moverHex, Hex targetHex)
    {
        CustomMoveAction moveAction = new CustomMoveAction();

        //IAction
        moveAction.RequestingPlayerID = requestingPlayerID;
        moveAction.ActorCharacter = moverCharacter;
        moveAction.ActorHex = moverHex;
        moveAction.RequestingClient = sender;

        //ITargetedAction
        moveAction.TargetHex = targetHex;
        moveAction.AllowedTargetTypes = new List<TargetType> { TargetType.empty_hex };
        moveAction.RequiresLOS = false;
        moveAction.Range = Utility.MAX_DISTANCE_ON_MAP;

        //IMoveAction
        moveAction.MoverStats = moverCharacter.CurrentStats;

        return moveAction;
    }

    //For attacking characters
    public static IAttackAction CreateAttackAction(NetworkConnectionToClient sender,
                                                   int requestingPlayerID,
                                                   PlayerCharacter attackerCharacter,
                                                   CharacterStats attackerStats,
                                                   Hex attackerHex,
                                                   Hex defenderHex)
    {
        Type attackActionType = ClassDataSO.Singleton.GetAttackActionTypeByID(attackerCharacter.charClass.attackActionID);
        IAttackAction attackAction = (IAttackAction) Activator.CreateInstance(attackActionType);

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
        attackAction.Damage = attackerStats.damage;
        attackAction.DamageIterations = attackerStats.damageIterations;
        attackAction.AttackDamageType = attackerStats.damageType;
        attackAction.PenetratingDamage = attackerStats.penetratingDamage;
        attackAction.Knockback = attackerStats.knockback;
        attackAction.CritChance = attackerStats.critChance;
        attackAction.CritMultiplier = attackerStats.critMultiplier;
        attackAction.TargetedAreaType = attackerStats.attackAreaType;
        attackAction.AreaScaler = attackerStats.attackAreaScaler;
        attackAction.DamageSourceName = "Attack";

        return attackAction;
    }

    public static IAbilityAction CreateAbilityAction(NetworkConnectionToClient sender, int requestingPlayerID, PlayerCharacter actingCharacter, CharacterAbilityStats ability, Hex userHex, Hex targetHex)
    {
        if(ability.isPassive)
        {
            throw new Exception("Attempting to create action for passive ability. No can do.");
        }
        Type abilityActionType = ClassDataSO.Singleton.GetAbilityActionTypeByID(ability.stringID);
        IAbilityAction abilityAction = (IAbilityAction) Activator.CreateInstance(abilityActionType);

        //IAction
        abilityAction.RequestingPlayerID = requestingPlayerID;
        abilityAction.ActorCharacter = actingCharacter;
        abilityAction.ActorHex = userHex;
        abilityAction.RequestingClient = sender;

        //IAbilityAction
        abilityAction.AbilityStats = ability;
        
        //ITargetedAction (conditionally)
        //Assign target hex if ability implements ITargetedAction
        if (typeof(ITargetedAction).IsAssignableFrom(abilityActionType))
        {
            ITargetedAction actionWithTarget = abilityAction as ITargetedAction;
            actionWithTarget.TargetHex = targetHex;
            actionWithTarget.AllowedTargetTypes = ability.allowedAbilityTargets;
            actionWithTarget.Range = ability.range;
            actionWithTarget.RequiresLOS = ability.requiresLOS;
        }

        if (typeof(IAreaTargeter).IsAssignableFrom(abilityActionType))
        {
            IAreaTargeter actionWithTargetedArea = abilityAction as IAreaTargeter;
            actionWithTargetedArea.TargetedAreaType = ability.areaType;
            actionWithTargetedArea.AreaScaler = ability.areaScaler;

        }

        return abilityAction;
    }

    internal static CustomAttackAction CreateCustomAttackAction(NetworkConnectionToClient sender,
                                                                 int requestingPlayerID,
                                                                 PlayerCharacter attackingCharacter,
                                                                 int damage,
                                                                 DamageType damageType,
                                                                 int damageIterations,
                                                                 bool penetratingDamage,
                                                                 int knockback,
                                                                 bool canCrit,
                                                                 float critChance,
                                                                 float critMultiplier,
                                                                 AreaType areaType,
                                                                 int areaScaler,
                                                                 Hex source,
                                                                 string damageSourceName,
                                                                 Hex primaryTarget)
    {
        CustomAttackAction customAttackAction = new CustomAttackAction();

        //IAction
        customAttackAction.RequestingPlayerID = requestingPlayerID;
        customAttackAction.ActorCharacter = attackingCharacter;
        customAttackAction.ActorHex = source;
        customAttackAction.RequestingClient = sender;

        //ITargetedAction
        //We are trusting main ability to assign valid targets to these sub actions since we might want to use different criteria than those main action
        customAttackAction.TargetHex = primaryTarget;
        customAttackAction.AllowedTargetTypes = Utility.GetAllEnumValues<TargetType>();
        customAttackAction.RequiresLOS = false;
        customAttackAction.Range = 99;

        customAttackAction.Damage = damage;
        customAttackAction.DamageIterations = damageIterations;
        customAttackAction.AttackDamageType = damageType;
        customAttackAction.PenetratingDamage = penetratingDamage;
        customAttackAction.Knockback = knockback;
        //use attack stats if -1 and can crit
        if (canCrit)
        {
            customAttackAction.CritChance = critChance;
            customAttackAction.CritMultiplier = critMultiplier;
        }
        else
        {
            customAttackAction.CritChance = 0f;
            customAttackAction.CritMultiplier = 1f;
        }

        customAttackAction.TargetedAreaType = areaType;
        customAttackAction.AreaScaler = areaScaler;
        customAttackAction.DamageSourceName = damageSourceName;

        return customAttackAction;
    }

    internal static BallistaAttackAction CreateBallistaAttackAction(NetworkConnectionToClient sender,
                                                             int requestingPlayerID,
                                                             PlayerCharacter attackingCharacter,
                                                             int damage,
                                                             DamageType damageType,
                                                             int damageIterations,
                                                             int range,
                                                             bool penetratingDamage,
                                                             int knockback,
                                                             bool canCrit,
                                                             float critChance,
                                                             float critMultiplier,
                                                             AreaType areaType,
                                                             int areaScaler,
                                                             Hex source,
                                                             Hex primaryTarget)
    {
        BallistaAttackAction customAttackAction = new BallistaAttackAction();

        //IAction
        customAttackAction.RequestingPlayerID = requestingPlayerID;
        customAttackAction.ActorCharacter = attackingCharacter;
        customAttackAction.ActorHex = source;
        customAttackAction.RequestingClient = sender;

        //ITargetedAction
        //We are trusting main ability to assign valid targets to these sub actions since we might want to use different criteria than those main action
        customAttackAction.TargetHex = primaryTarget;
        customAttackAction.AllowedTargetTypes = Utility.GetAllEnumValues<TargetType>();
        customAttackAction.RequiresLOS = false;
        customAttackAction.Range = range;

        //IAttackAction
        customAttackAction.Damage = damage;
        customAttackAction.DamageIterations = damageIterations;
        customAttackAction.AttackDamageType = damageType;
        customAttackAction.PenetratingDamage = penetratingDamage;
        customAttackAction.Knockback = knockback;
        customAttackAction.DamageSourceName = "Ballista shot";

        //use attack stats if -1 and can crit
        if (canCrit)
        {
            customAttackAction.CritChance = critChance;
            customAttackAction.CritMultiplier = critMultiplier;
        }
        else
        {
            customAttackAction.CritChance = 0f;
            customAttackAction.CritMultiplier = 1f;
        }

        customAttackAction.TargetedAreaType = areaType;
        customAttackAction.AreaScaler = areaScaler;

        return customAttackAction;
    }


    public static CustomAttackAction CreateAbilityAttackAction(NetworkConnectionToClient sender,
                                                                int requestingPlayerID,
                                                                PlayerCharacter attackerCharacter,
                                                                CharacterStats attackerStats,
                                                                CharacterAbilityStats abilityStats,
                                                                Hex attackerHex,
                                                                Hex targetedHex)
    {
        CustomAttackAction abilityAttackAction = new CustomAttackAction();

        //IAction
        abilityAttackAction.RequestingPlayerID = requestingPlayerID;
        abilityAttackAction.ActorCharacter = attackerCharacter;
        abilityAttackAction.ActorHex = attackerHex;
        abilityAttackAction.RequestingClient = sender;

        //ITargetedAction
        //We are trusting main ability to assign valid targets to these sub actions since we might want to use different criteria than those for main ability action
        abilityAttackAction.TargetHex = targetedHex;
        abilityAttackAction.AllowedTargetTypes = Utility.GetAllEnumValues<TargetType>();
        abilityAttackAction.RequiresLOS = false;
        abilityAttackAction.Range = 99;

        //IAttackAction from ability stats
        abilityAttackAction.Damage = abilityStats.damage == -1 ? attackerStats.damage : abilityStats.damage;
        abilityAttackAction.DamageIterations = abilityStats.damageIterations;
        abilityAttackAction.AttackDamageType = abilityStats.damageType;
        abilityAttackAction.PenetratingDamage = abilityStats.penetratingDamage;
        abilityAttackAction.Knockback = abilityStats.knockback;
        //use attack stats if -1 and can crit
        if (abilityStats.canCrit)
        {
            abilityAttackAction.CritChance = abilityStats.critChance == -1f ? attackerStats.critChance : abilityStats.critChance;
            abilityAttackAction.CritMultiplier = abilityStats.critMultiplier == -1f ? attackerStats.critMultiplier : abilityStats.critMultiplier;
        } else
        {
            abilityAttackAction.CritChance = 0f;
            abilityAttackAction.CritMultiplier = 1f;
        }
        
        abilityAttackAction.TargetedAreaType = abilityStats.areaType;
        abilityAttackAction.AreaScaler = abilityStats.areaScaler;
        abilityAttackAction.DamageSourceName = abilityStats.interfaceName;

        return abilityAttackAction;
    }
}
