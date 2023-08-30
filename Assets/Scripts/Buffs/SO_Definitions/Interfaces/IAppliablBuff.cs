using System.Collections.Generic;
using UnityEngine;

public interface IAppliablBuff : IBuffDataSO
{        
    public bool NeedsToBeReAppliedEachTurn { get;}

    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication);

    public void UnApply(List<int> unApplyToCharacterIDs);

}
