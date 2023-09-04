using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStealthModifier : IStatModifier
{
    public bool StealthOffset { get; set; }
}
