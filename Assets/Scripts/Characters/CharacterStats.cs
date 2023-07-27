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

    public readonly float initiative;

    public readonly int range;

    public readonly List<TargetType> allowedAttackTargets;

    public CharacterStats(int maxHealth,
                          int armor,
                          int damage,
                          int moveSpeed,
                          int initiative,
                          float critChance = 0f,
                          float critMultiplier = 1f,
                          int range = 1,
                          int damageIterations = 1,
                          DamageType damageType = DamageType.physical,
                          List<TargetType> allowedAttackTargets = null)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.damage = damage;
        this.damageType = damageType;
        this.damageIterations = damageIterations;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
        this.moveSpeed = moveSpeed;
        this.initiative = initiative;
        this.range = range;
        if(allowedAttackTargets == null)
        {
            this.allowedAttackTargets = new List<TargetType> { TargetType.ennemy_chars, TargetType.obstacle };
        } else
        {
            this.allowedAttackTargets = new List<TargetType>(allowedAttackTargets);
        }
    }

    public CharacterStats(CharacterStats toCopy, 
                      int maxHealth = -1,
                      int armor = -1,
                      int damage = -1,
                      int moveSpeed = -1,
                      int initiative = -1,
                      float critChance = -1f,
                      float critMultiplier = -1f,
                      int range = -1,
                      int damageIterations = -1,
                      DamageType damageType = DamageType.none,
                      List<TargetType> allowedAttackTargets = null)
    {
        this.maxHealth = maxHealth == -1 ? toCopy.maxHealth : maxHealth;
        this.armor = armor == -1 ? toCopy.armor : armor;
        this.damage = damage == -1 ? toCopy.damage : damage;
        this.damageType = damageType == DamageType.none ? toCopy.damageType : damageType;
        this.damageIterations = damageIterations == -1 ? toCopy.damageIterations : damageIterations;
        this.critChance = critChance == -1f ? toCopy.critChance : critChance;
        this.critMultiplier = critMultiplier == -1f ? toCopy.critMultiplier : critMultiplier;
        this.moveSpeed = moveSpeed == -1 ? toCopy.moveSpeed : moveSpeed;
        this.initiative = initiative == -1 ? toCopy.initiative : initiative;
        this.range = range == -1 ? toCopy.range : range;
        this.allowedAttackTargets = allowedAttackTargets == null ? toCopy.allowedAttackTargets : this.allowedAttackTargets = allowedAttackTargets;
    }

    public Dictionary<string, string> GetPrintableDictionary()
    {
        Dictionary<string, string> toPrint = new();

        toPrint.Add("Health", String.Format("{0}", this.maxHealth));
        toPrint.Add("Armor", String.Format("{0}", this.armor));
        toPrint.Add("Damage", String.Format("{0} x {1} ({2})", this.damage, this.damageIterations, this.damageType));
        toPrint.Add("Crit", String.Format("{0}% (+{1}%)", this.critChance * 100, this.critMultiplier * 100));
        toPrint.Add("Range", String.Format("{0}", this.range));
        toPrint.Add("Moves", String.Format("{0}", this.moveSpeed));
        toPrint.Add("Initiative", String.Format("{0}", this.initiative));

        return toPrint;
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
            this.range == other.range)
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