using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballista : MonoBehaviour
{
    public int damage;
    public DamageType damageType;
    public int damageIterations;
    public int range;
    public float critChance;
    public float critMultiplier;
    public List<TargetType> allowedAttackTargets;
    public bool attacksRequireLOS;
    public bool penetratingDamage;
    public int knockback;
    public AreaType attackAreaType;
    public int attackAreaScaler;
    public float vulnerabilityMultiplier;
}
