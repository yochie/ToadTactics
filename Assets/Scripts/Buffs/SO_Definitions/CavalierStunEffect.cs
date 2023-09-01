using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CavalierStunEffect : StunEffectBase
{
    //public override string BuffTypeID => "CavalierStunEffect";
    //public override string UIName => "Cavalier Stun";
    //public override string IconName => "stun";
    public override bool NeedsToBeReAppliedEachTurn { get; set; }

    public override string stringID { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override string UIName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override bool IsPositive { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override DurationType DurationType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override int TurnDuration { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override Sprite Icon { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override string DescriptionFormat { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
