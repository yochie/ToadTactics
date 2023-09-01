using System.Collections.Generic;
using UnityEngine;

public interface IAppliablBuffDataSO : IBuffDataSO
{        
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    public void Apply(List<int> applyToCharacterIDs, bool isReapplication);

    public void UnApply(List<int> unApplyFromCharacterIDs);

}
