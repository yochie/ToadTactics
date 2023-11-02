using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TreasureSpawnLocation : NetworkBehaviour
{
    public Vector2Int treasureCoordinate;
    public Vector2Int ballistaCoordinate;
    public List<Vector2Int> hazardCoordinates;
    public List<Vector2Int> obstacleCoordinates;
}
