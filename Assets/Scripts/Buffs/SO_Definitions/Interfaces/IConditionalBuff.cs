using UnityEngine;

public interface IConditionalBuff
{
    public GameEventSO EndEvent { get; set; }
    string InlineConditionDescription { get; set; }

    public void OnEndEvent();
}
