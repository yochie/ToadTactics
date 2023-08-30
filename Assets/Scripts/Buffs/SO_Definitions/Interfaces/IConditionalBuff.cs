using UnityEngine;

public interface IConditionalBuff
{
    public GameEventSO EndEvent { get; set; }

    public void OnEndEvent();
}
