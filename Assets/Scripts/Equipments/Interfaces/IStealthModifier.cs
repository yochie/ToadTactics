using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStealthModifier : IStatModifier
{
    public int StealthLayersOffset { get; set; }
}
