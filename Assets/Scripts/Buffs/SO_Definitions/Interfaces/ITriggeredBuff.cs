
public interface ITriggeredBuff : IBuffDataSO
{
    void SetupListeners(RuntimeBuff runtimeBuff);

    void RemoveListenersForBuff(RuntimeBuff runtimeBuff);

    public int MaxTriggers { get; set; }
}
