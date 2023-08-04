using UnityEngine;

public interface IConditionalEffect
{
    public Object ListensToEventsRaisedBy { get; set; }
    public GameEventSO EndEvent { get; set; }

    public void OnEndEvent();
}
