using System.Collections.Generic;
using UnityEngine;

public interface IAppliedBuff : IBuff, IDisplayedBuff
{        
    public bool IsPositive { get; }

    public bool NeedsToBeReAppliedEachTurn { get;}

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication);

    public void UnApply(List<int> unApplyToCharacterIDs);

}
