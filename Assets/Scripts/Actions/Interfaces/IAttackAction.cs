
using System.Collections.Generic;

public interface IAttackAction : ITargetedAction, IAreaTargeter, IPreviewedAction
{ 
    //public CharacterStats AttackerStats { get; set; }
    public int Damage { get; set; }
    public int DamageIterations { get; set; }
    public DamageType AttackDamageType { get; set; }
    public bool PenetratingDamage { get; set; }
    public int Knockback { get; set; }
    public float CritChance { get; set; }
    public float CritMultiplier { get; set; }

    public string DamageSourceName { get; set; }

}
