using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmorModifier : IStatModifier
{
    public int ArmorOffset { get; set; }
}
