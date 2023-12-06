using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class MapObstacleManager : NetworkBehaviour
{
    //Server only
    private Dictionary<Vector2Int, Obstacle> spawnedObstacles = new();

    [SerializeField]
    private List<Obstacle> obstaclePrefabs;

    [Server]
    internal void SpawnObstacleOnMap(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates, ObstacleType type)
    {
        Hex targetHex = Map.GetHex(grid, coordinates);
        if (targetHex.HoldsAnObstacle())
        {
            this.RemoveObstacleAtPosition(grid, coordinates);
        }
        targetHex.SetObstacle(type);
        this.RpcDisplayObstacleSprite(coordinates, type);
    }

    [Server]
    public void RemoveObstacleAtPosition(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates)
    {
        Hex obstacleHex = Map.GetHex(grid, coordinates);
        if (!obstacleHex.HoldsAnObstacle())
        {
            throw new System.Exception("Trying to destroy obstacle at hex that doesn't contain one..");
        }
        obstacleHex.ClearObstacle();
        this.RpcDestroyObstacleSprite(coordinates);

    }

    private Obstacle GetPrefabForType(ObstacleType type)
    {
        return this.obstaclePrefabs.Single<Obstacle>(obstacle => obstacle.type == type);
    }

    [ClientRpc]
    private void RpcDisplayObstacleSprite(Vector2Int coordinates, ObstacleType type)
    {
        AnimationSystem.Singleton.Queue(this.DisplayObstacleCoroutine(coordinates, type));
    }

    private IEnumerator DisplayObstacleCoroutine(Vector2Int coordinates, ObstacleType type)
    {
        Dictionary<Vector2Int, Hex> grid = Map.Singleton.hexGrid;
        Hex obstacleHex = Map.GetHex(grid, coordinates.x, coordinates.y);

        Obstacle obstacleTypePrefab = this.GetPrefabForType(type);
        GameObject obstaceObject = Instantiate(obstacleTypePrefab.gameObject, obstacleHex.transform.position, Quaternion.identity);
        this.spawnedObstacles[coordinates] = obstaceObject.GetComponent<Obstacle>();
        yield break;
    }

    [ClientRpc]
    private void RpcDestroyObstacleSprite(Vector2Int coordinates)
    {
        AnimationSystem.Singleton.Queue(this.DestroyObstacleSpriteCoroutine(coordinates));
    }

    private IEnumerator DestroyObstacleSpriteCoroutine(Vector2Int coordinates)
    {
        Obstacle obstacleToDestroy = this.spawnedObstacles[coordinates];
        Destroy(obstacleToDestroy.gameObject);
        this.spawnedObstacles.Remove(coordinates);
        yield break;
    }
}
