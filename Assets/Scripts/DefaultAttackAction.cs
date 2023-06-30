using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class DefaultAttackAction : IAction
{
    private PlayerCharacter attacker;
    private PlayerCharacter defender;
    private Hex attackerHex;
    private Hex targetedHex;
    private CharacterStats attackerStats;
    private CharacterStats defenderStats;

    public DefaultAttackAction(PlayerCharacter attacker, PlayerCharacter defender, Hex attackerHex, Hex targetedHex, CharacterStats attackerStats, CharacterStats defenderStats)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.attackerHex = attackerHex;
        this.targetedHex = targetedHex;
        this.attackerStats = attackerStats;
        this.defenderStats = defenderStats;
    }

    [Command(requiresAuthority = false)]
    public void CmdUse()
    {
        int prevLife = defender.CurrentLife();
        for (int i = 0; i < attackerStats.damageIterations; i++)
        {
            switch (attackerStats.damageType)
            {
                case DamageType.normal:
                    defender.TakeRawDamage(attackerStats.damage - defenderStats.armor);
                    break;
                case DamageType.magic:
                    defender.TakeRawDamage(attackerStats.damage);
                    break;
                case DamageType.healing:
                    defender.TakeRawDamage(-attackerStats.damage);
                    break;
            }
        }

        //use PlayerCharacter attack action
        attacker.UsedAttack();

        Debug.LogFormat("{0} has attacked {1} for {2}x{3} leaving him with {4} => {5} life.", attacker, defender, attackerStats.damage, attackerStats.damageIterations, prevLife, defender.CurrentLife());
    }

    public bool Validate()
    {
        if (!Map.Singleton.LOSReaches(attackerHex, targetedHex, attackerStats.range) ||
            attacker.hasAttacked ||
            !GameController.Singleton.IsItMyTurn() ||
            !GameController.Singleton.IsItThisCharactersTurn(attacker.charClassID) ||
            !GameController.Singleton.DoIOwnThisCharacter(attacker.charClassID) ||
            !IsValidTargetType(attacker, defender, targetedHex, attackerStats.allowedAttackTargets)
            )
            return false;
        else
            return true;
    }

    private bool IsValidTargetType (PlayerCharacter user, PlayerCharacter target, Hex targetHex, List<TargetType> allowedTargets)
    {
        bool selfTarget = attacker.charClassID == defender.charClassID;
        bool friendlyTarget = attacker.ownerID == defender.ownerID;
        bool ennemyTarget = !friendlyTarget;
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
