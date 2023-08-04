using UnityEngine;

public interface IBuffEffect
{
    //Set in class definition
    public string StringID { get;}

    public bool IsPositive { get; }

    public string IconName{ get; }

    public string UIName { get; }

    public bool NeedsToBeReAppliedEachTurn { get;}

    //Set at runtime
    public int AffectedCharacterID { get; set; }

    public bool ApplyEffect(bool isReapplication);

    public void UnApply();

}
