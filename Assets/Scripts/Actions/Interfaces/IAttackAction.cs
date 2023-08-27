
using System.Collections.Generic;

public interface IAttackAction : ITargetedAction
{ 
    //public CharacterStats AttackerStats { get; set; }
    public int Damage { get; set; }
    public int DamageIterations { get; set; }
    public DamageType AttackDamageType { get; set; }
    public bool PenetratingDamage { get; set; }
    public bool KnocksBack { get; set; }
    public float CritChance { get; set; }
    public float CritMultiplier { get; set; }
    public AreaType AttackAreaType { get; set; }
    public int AttackAreaScaler { get; set; }
    public List<Hex> SecondaryTargetedHexes { get; set; }
}
