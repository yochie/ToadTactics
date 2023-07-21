using System.Collections.Generic;
using System;

public readonly struct CharacterStats
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
                          DamageType damageType = DamageType.normal,
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
            this.allowedAttackTargets = allowedAttackTargets;
        }
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

}