using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WarriorRootEffect : RootEffectBase, IAbilityBuffEffect
{
    public override string BuffTypeID => "WarriorRootEffect";
    public override string UIName => "Warrior fear";
    public override string IconName => "root";

    //IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
