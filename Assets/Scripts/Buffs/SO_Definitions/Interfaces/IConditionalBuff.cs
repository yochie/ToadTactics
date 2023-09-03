using UnityEngine;

public interface IConditionalBuff
{
    string InlineConditionDescription { get; set; }

    public void OnEndEvent();
}
