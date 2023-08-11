using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitiativeModifier : IStatModifier
{
    public float InitiativeOffset { get; set; }
}
