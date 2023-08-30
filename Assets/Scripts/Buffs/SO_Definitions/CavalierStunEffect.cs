using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CavalierStunEffect : StunEffectBase
{
    //public override string BuffTypeID => "CavalierStunEffect";
    //public override string UIName => "Cavalier Stun";
    //public override string IconName => "stun";
    public override bool NeedsToBeReAppliedEachTurn => throw new System.NotImplementedException();

    public override string BuffTypeID { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override string UIName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override bool IsPositive { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override DurationType DurationType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override int TurnDuration { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override Image Icon { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override string tooltipDescription { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
