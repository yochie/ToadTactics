using System;
using System.Collections.Generic;

public readonly struct CharacterAbilityStats
{
    public readonly string stringID;
    public readonly string interfaceName;
    public readonly string description;
    public readonly int damage;
    public readonly int damageIterations;
    public readonly DamageType damageType;
    public readonly int range;
    public readonly AreaType areaType;
    public readonly int areaScaler;
    public readonly bool requiresLOS;
    public readonly List<TargetType> allowedAbilityTargets;
    public readonly bool canCrit;
    //-1 = use character stat
    public readonly float critChance;
    //-1 = use character stat
    public readonly float critMultiplier;
    //-1 = infinite
    public readonly int usesPerRound;
    public readonly int cooldownDuration;
    public readonly bool isPassive;
    public readonly bool penetratingDamage;
    public readonly bool cappedPerRound;
    public readonly bool cappedByCooldown;
    public readonly int knockback;
    public readonly string appliesSelfBuffOnRoundStart;
    public readonly string passiveGrantsAltAttack;
    public readonly string passiveGrantsAltMove;
    public readonly string passiveCanCauseBuff;


    //-1 to crit chance or crit multi means use attacker stats if canCrit
    public CharacterAbilityStats(string stringID,
                            string interfaceName,
                            string description,
                            int damage = 0,
                            int range = -1,
                            int areaScaler = -1,
                            bool requiresLOS = true,
                            int damageIterations = -1,
                            DamageType damageType = DamageType.none,
                            List<TargetType> allowedAbilityTargets = null,
                            bool canCrit = false,
                            float critChance = -1f,
                            float critMultiplier = -1f,
                            int usesPerRound = -1,
                            int cooldownDuration = -1,
                            bool isPassive = false,
                            bool penetratingDamage = false,
                            bool cappedPerRound = false,
                            bool cappedByCooldown = false,
                            int knocksBack = 0,
                            AreaType areaType = default,
                            string appliesBuffIDOnRoundStart = null,
                            string passiveGrantsAltAttack = null, 
                            string passiveGrantsAltMove = null, 
                            string passiveCanCauseBuff = null)
    {
        this.stringID = stringID;
        this.interfaceName = interfaceName;
        this.description = description;
        this.damage = damage;
        this.range = range;
        this.areaScaler = areaScaler;
        this.requiresLOS = requiresLOS;
        this.damageIterations = damageIterations;
        this.damageType = damageType;

        if (allowedAbilityTargets == null)
        {
            this.allowedAbilityTargets = new() { TargetType.ennemy_chars, TargetType.obstacle };
        }
        else
        {
            this.allowedAbilityTargets = new List<TargetType>(allowedAbilityTargets);
        }

        this.canCrit = canCrit;
        this.critChance = critChance;
        this.critMultiplier = critMultiplier;
        this.usesPerRound = usesPerRound;
        this.cooldownDuration = cooldownDuration;
        this.isPassive = isPassive;
        this.penetratingDamage = penetratingDamage;
        this.cappedPerRound = cappedPerRound;
        this.cappedByCooldown = cappedByCooldown;
        this.knockback = knocksBack;
        this.areaType = areaType;
        this.appliesSelfBuffOnRoundStart = appliesBuffIDOnRoundStart;
        this.passiveGrantsAltAttack = passiveGrantsAltAttack;
        this.passiveGrantsAltMove = passiveGrantsAltMove;
        this.passiveCanCauseBuff = passiveCanCauseBuff;
    }

    public IBuffDataSO GetActivatedBuff()
    {
        IBuffDataSO buff = null;
        if (!this.isPassive)
        {
            Type abilityType = ClassDataSO.Singleton.GetAbilityActionTypeByID(this.stringID);

            //Need to create instance since buff data isn't stored statically in ability objects... this is issue caused by not having ability data stored as SO
            IAbilityAction abilityAction = (IAbilityAction)Activator.CreateInstance(abilityType);
            IActivatedBuffSource buffingAction = abilityAction as IActivatedBuffSource;
            if (buffingAction != null)
            {
                buff = buffingAction.AppliesBuffOnActivation;
            }
        } else if (passiveCanCauseBuff != null)
        {
            buff = BuffDataSO.Singleton.GetBuffData(this.passiveCanCauseBuff);
        }

        return buff;
    }

    public IBuffDataSO GetPassiveBuff()
    {
        IBuffDataSO buff = null;
        if (this.isPassive && this.appliesSelfBuffOnRoundStart != null)
        {
            buff = BuffDataSO.Singleton.GetBuffData(this.appliesSelfBuffOnRoundStart);
        }
        return buff;
    }
}
