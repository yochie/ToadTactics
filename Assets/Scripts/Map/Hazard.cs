using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Hazard : NetworkBehaviour
{
    [SyncVar]
    public HazardType type;
}
