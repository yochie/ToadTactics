using System.Collections.Generic;
using System;
using Mirror;

public readonly struct CharacterStats : IEquatable<CharacterStats>
{

    public readonly int maxHealth;

    public readonly int armor;

    public readonly int damage;

    public readonly DamageType damageType;

    public readonly int damageIterations;

    public readonly float critChance;

    public readonly float critMultiplier;

    public readonly int moveSpeed;

    public readonly int attacksPerTurn;

    public readonly float initiative;

    public readonly int range;

    public readonly List<TargetType> allowedAttackTargets;

    public readonly bool attacksRequireLOS;

    public readonly int kingDamage;


    public CharacterStats(int maxHealth,
                          int armor,
                          int damage,
                          int moveSpeed,
                          int initiative,
                          bool attacksRequireLOS = true,
                          int attacksPerTurn = 1,
                          float critChance = 0f,
                          float critMultiplier = 1f,
                          int range = 1,
                          int damageIterations = 1,
                          DamageType damageType = DamageType.physical,
                          List<TargetType> allowedAttackTargets = null,
                          int? kingDamage = null)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.damage = damage;
        this.damageType = damageType;
        this.damageIterations = damageIterations;
        this.attacksPerTurn = attacksPerTurn;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
        this.moveSpeed = moveSpeed;
        this.initiative = initiative;
        this.range = range;
        this.attacksRequireLOS = attacksRequireLOS;
        if(allowedAttackTargets == null)
        {
            this.allowedAttackTargets = new List<TargetType> { TargetType.ennemy_chars, TargetType.obstacle };
        } else
        {
            this.allowedAttackTargets = new List<TargetType>(allowedAttackTargets);
        }
        if (kingDamage == null)
            this.kingDamage = damage;
        else
            this.kingDamage = kingDamage.GetValueOrDefault();

    }

    public CharacterStats(CharacterStats toCopy,
                      int maxHealth = -1,
                      int armor = -1,
                      int damage = -1,
                      int attacksPerTurn = -1,
                      int abilitiesPerTurn = -1,
                      int moveSpeed = -1,
                      int initiative = -1,
                      float critChance = -1f,
                      float critMultiplier = -1f,
                      int range = -1,
                      int damageIterations = -1,
                      bool? attacksRequireLOS = null,
                      DamageType damageType = DamageType.none,
                      List<TargetType> allowedAttackTargets = null,
                      int? kingDamage = null)
    {
        this.maxHealth = maxHealth == -1 ? toCopy.maxHealth : maxHealth;
        this.armor = armor == -1 ? toCopy.armor : armor;
        this.damage = damage == -1 ? toCopy.damage : damage;
        this.attacksPerTurn = attacksPerTurn == -1 ? toCopy.attacksPerTurn : attacksPerTurn;
        this.damageType = damageType == DamageType.none ? toCopy.damageType : damageType;
        this.damageIterations = damageIterations == -1 ? toCopy.damageIterations : damageIterations;
        this.critChance = critChance == -1f ? toCopy.critChance : critChance;
        this.critMultiplier = critMultiplier == -1f ? toCopy.critMultiplier : critMultiplier;
        this.moveSpeed = moveSpeed == -1 ? toCopy.moveSpeed : moveSpeed;
        this.initiative = initiative == -1 ? toCopy.initiative : initiative;
        this.range = range == -1 ? toCopy.range : range;
        this.allowedAttackTargets = allowedAttackTargets == null ? toCopy.allowedAttackTargets : allowedAttackTargets;
        //nullable bool requires conversion
        this.attacksRequireLOS = attacksRequireLOS == null ? toCopy.attacksRequireLOS : attacksRequireLOS.GetValueOrDefault();
        this.kingDamage = kingDamage == null ? toCopy.kingDamage : kingDamage.GetValueOrDefault();
    }

    public bool Equals(CharacterStats other)
    {

        if (this.maxHealth == other.maxHealth &&
            this.armor == other.armor &&
            this.damage == other.damage &&
            this.damageType == other.damageType &&
            this.damageIterations == other.damageIterations &&
            this.critChance == other.critChance &&
            this.critMultiplier == other.critMultiplier &&
            this.moveSpeed == other.moveSpeed &&
            this.initiative == other.initiative &&
            this.range == other.range &&
            this.attacksRequireLOS == other.attacksRequireLOS &&
            this.kingDamage == other.kingDamage)
        {
            foreach (TargetType t in this.allowedAttackTargets)
            {
                if (!other.allowedAttackTargets.Contains(t))
                    return false;                
            }
            return true;
        } else
        {
            return false;
        }
    }
}