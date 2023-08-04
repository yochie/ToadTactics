using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Effect", menuName = "Buffs/CavalierStunEffect")]
public class CavalierStunEffect : StunEffectBase
{
    [field: SerializeField]
    public override string StringID { get; set; }
    [field: SerializeField]
    public override string UIName { get; set; }
    [field: SerializeField]
    public override Sprite Icon { get; set; }
    [field: SerializeField]
    public override bool NeedsToBeReAppliedEachTurn { get; set; }
    [field: SerializeField]
    public override bool IsPositive { get; set; }

    public override int AppliedToCharacterID { get; set; }

    public override int TurnDurationRemaining { get; set; }
}
