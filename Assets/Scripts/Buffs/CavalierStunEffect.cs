using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CavalierStunEffect : StunEffectBase, IAbilityBuffEffect, IDisplayedBuff, IAppliedBuff
{
    public override string BuffTypeID => "CavalierStunEffect";
    public override string UIName => "Cavalier Stun";
    public override string IconName => "stun";

    //IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }

    public Dictionary<string, string> GetAbilityBuffPrintoutDictionnary()
    {
        Dictionary<string, string> printouts = new();
        printouts.Add("Stun duration", "1 turn");

    }
}
