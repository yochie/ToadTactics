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

    public readonly bool penetratingDamage;

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
                          int? kingDamage = null,
                          bool penetratingDamage = false
                          )
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
        this.penetratingDamage = penetratingDamage;

    }

    public CharacterStats(CharacterStats toCopy,
                      int? maxHealth = null,
                      int? armor = null,
                      int? damage = null,
                      int? attacksPerTurn = null,
                      int? moveSpeed = null,
                      int? initiative = null,
                      float? critChance = null,
                      float? critMultiplier = null,
                      int? range = null,
                      int? damageIterations = null,
                      bool? attacksRequireLOS = null,
                      DamageType? damageType = null,
                      int? kingDamage = null,
                      bool? penetratingDamage = null,
                      List<TargetType> allowedAttackTargets = null
                      )
    {
        this.maxHealth = maxHealth == null ? toCopy.maxHealth : maxHealth.GetValueOrDefault();
        this.armor = armor == null ? toCopy.armor : armor.GetValueOrDefault();
        this.damage = damage == null ? toCopy.damage : damage.GetValueOrDefault();
        this.attacksPerTurn = attacksPerTurn == null ? toCopy.attacksPerTurn : attacksPerTurn.GetValueOrDefault();
        this.moveSpeed = moveSpeed == null ? toCopy.moveSpeed : moveSpeed.GetValueOrDefault();
        this.initiative = initiative == null ? toCopy.initiative : initiative.GetValueOrDefault();
        this.critChance = critChance == null ? toCopy.critChance : critChance.GetValueOrDefault();
        this.critMultiplier = critMultiplier == null ? toCopy.critMultiplier : critMultiplier.GetValueOrDefault();
        this.range = range == null ? toCopy.range : range.GetValueOrDefault();
        this.damageIterations = damageIterations == null ? toCopy.damageIterations : damageIterations.GetValueOrDefault();
        this.attacksRequireLOS = attacksRequireLOS == null ? toCopy.attacksRequireLOS : attacksRequireLOS.GetValueOrDefault();
        this.damageType = damageType == null ? toCopy.damageType : damageType.GetValueOrDefault();
        this.kingDamage = kingDamage == null ? toCopy.kingDamage : kingDamage.GetValueOrDefault();
        this.penetratingDamage = penetratingDamage == null ? toCopy.penetratingDamage : penetratingDamage.GetValueOrDefault();

        this.allowedAttackTargets = allowedAttackTargets == null ? toCopy.allowedAttackTargets : allowedAttackTargets;

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
            this.kingDamage == other.kingDamage &&
            this.penetratingDamage == other.penetratingDamage
            )
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