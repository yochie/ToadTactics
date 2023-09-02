using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIntEventTriggeredBuff : ITriggeredBuff
{
    public IntGameEventSO TriggerEvent { get; set; }
}
