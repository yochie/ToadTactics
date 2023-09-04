
using System.Collections.Generic;

public interface ITriggeredBuff : IBuffDataSO
{
    void SetupTriggerListeners(RuntimeBuff runtimeBuff);

    void RemoveTriggerListenersForBuff(RuntimeBuff runtimeBuff, List<int> removeFromCharacters);

    public int MaxTriggers { get; set; }
}
