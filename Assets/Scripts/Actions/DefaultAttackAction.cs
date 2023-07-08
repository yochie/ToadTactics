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
    private int attackingPlayerID;

    public DefaultAttackAction(PlayerCharacter attacker, PlayerCharacter defender, Hex attackerHex, Hex targetedHex, CharacterStats attackerStats, CharacterStats defenderStats, int attackingPlayerID)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.attackerHex = attackerHex;
        this.targetedHex = targetedHex;
        this.attackerStats = attackerStats;
        this.defenderStats = defenderStats;
        this.attackingPlayerID = attackingPlayerID;
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

        //PlayerCharacter state updated to track that attack was used
        attacker.UsedAttack();

        Debug.LogFormat("{0} has attacked {1} for {2}x{3} leaving him with {4} => {5} life.", attacker, defender, attackerStats.damage, attackerStats.damageIterations, prevLife, defender.CurrentLife());
    }

    public bool Validate()
    {
        if (!MapPathfinder.LOSReaches(this.attackerHex, this.targetedHex, this.attackerStats.range) ||
            this.attacker.hasAttacked ||
            !GameController.Singleton.IsItThisPlayersTurn(this.attackingPlayerID) ||
            !GameController.Singleton.IsItThisCharactersTurn(this.attacker.charClassID) ||
            !GameController.Singleton.DoesHeOwnThisCharacter(this.attackingPlayerID, this.attacker.charClassID) ||
            !this.IsValidTargetType(this.attacker, this.defender, this.targetedHex, this.attackerStats.allowedAttackTargets)
            )
            return false;
        else
            return true;
    }

    private bool IsValidTargetType (PlayerCharacter attacker, PlayerCharacter target, Hex targetHex, List<TargetType> allowedTargets)
    {
        bool selfTarget = attacker.charClassID == target.charClassID;
        bool friendlyTarget = attacker.ownerID == target.ownerID;
        bool ennemyTarget = !friendlyTarget;
        bool emptyTarget = !targetHex.HoldsACharacter() && targetHex.holdsObstacle == ObstacleType.none;
        bool obstacleTarget = targetHex.holdsObstacle != ObstacleType.none;

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
