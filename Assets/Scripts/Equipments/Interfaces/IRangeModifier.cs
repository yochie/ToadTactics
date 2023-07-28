using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRangeModifier : IStatModifier
{
    public int RangeOffset { get; set; }
}
