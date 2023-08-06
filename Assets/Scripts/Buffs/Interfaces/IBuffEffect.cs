using System.Collections.Generic;
using UnityEngine;

public interface IBuffEffect
{
    
    //Set in class definition
    public string BuffTypeID { get;}

    public bool IsPositive { get; }

    public string IconName{ get; }

    public string UIName { get; }

    public bool NeedsToBeReAppliedEachTurn { get;}

    //Set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    public bool ApplyEffect(bool isReapplication);

    public void UnApply();

}
