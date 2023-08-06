using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CavalierStunEffect : StunEffectBase, IAbilityBuffEffect
{
    public override string BuffTypeID => "CavalierStunEffect";
    public override string UIName => "Cavalier Stun";
    public override string IconName => "stun";

    //IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }
}
