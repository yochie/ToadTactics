using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hazard : MonoBehaviour
{
    [SerializeField]
    private HazardType type;

    public HazardType Type { get => this.type; }
}
