using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Obstacle : NetworkBehaviour
{
    [SyncVar]
    public ObstacleType type;

    [SyncVar]
    public HexCoordinates hexPosition;
}
