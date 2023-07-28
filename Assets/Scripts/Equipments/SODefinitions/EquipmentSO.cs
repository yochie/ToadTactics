using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentSO : ScriptableObject
{
    [field: SerializeField]
    public string EquipmentID { get; private set; }

    [field: SerializeField]
    public string NameUI { get; private set; }

    [SerializeField]
    public Sprite Sprite { get; private set; }

    [field: SerializeField]
    public string Description { get; private set; }
}
