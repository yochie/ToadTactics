using UnityEngine;

public interface IBuffEffect
{
    //Set in SO
    public string StringID { get; set; }

    public bool IsPositive { get; set; }

    public Sprite Icon { get; set; }

    public string UIName { get; set; }

    public bool NeedsToBeReAppliedEachTurn { get; set; }

    //Set at runtime
    public int AppliedToCharacterID { get; set; }

    public bool ApplyEffect(bool isReapplication);

    public void UnApply();

}
