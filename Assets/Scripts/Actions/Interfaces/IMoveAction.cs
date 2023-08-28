using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveAction : ITargetedAction
{
    public CharacterStats MoverStats { get; set; }
    public void SetupPath();
}
