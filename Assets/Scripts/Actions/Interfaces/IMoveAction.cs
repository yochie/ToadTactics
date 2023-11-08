using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveAction : ITargetedAction, IPreviewedAction
{
    public CharacterStats MoverStats { get; set; }
    public void SetupPath();
}
