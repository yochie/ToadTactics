using System.Collections.Generic;
using UnityEngine;

public interface IConditionalBuff : IBuffDataSO
{
    string InlineConditionDescription { get; set; }

    public void SetupConditionListeners(RuntimeBuff buff);
    public void RemoveConditionListenersForBuff(RuntimeBuff buff, List<int> removeFromCharacters);
}
