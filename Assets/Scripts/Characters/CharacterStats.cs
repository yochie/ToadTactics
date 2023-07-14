using System.Collections.Generic;
using System;

public readonly struct CharacterStats
{
    public readonly int maxHealth;
    public readonly int armor;
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly int damageIterations;
    public readonly int moveSpeed;
    public readonly float initiative;
    public readonly int range;
    public readonly List<TargetType> allowedAttackTargets;

    public CharacterStats(int maxHealth, int armor, int damage, int moveSpeed, int initiative, int range = 1, int damageIterations = 1, DamageType damageType = DamageType.normal, List<TargetType> allowedAttackTargets = null)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.damage = damage;
        this.damageType = damageType;
        this.damageIterations = damageIterations;
        this.moveSpeed = moveSpeed;
        this.initiative = initiative;
        this.range = range;
        if(allowedAttackTargets == null)
        {
            this.allowedAttackTargets = new List<TargetType> { TargetType.ennemy_chars, TargetType.obstacle };
        } else
        {
            this.allowedAttackTargets = allowedAttackTargets;
        }
    }

}