using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
public class MapObstacleManager : MonoBehaviour
{
    //Server only
    private Dictionary<Vector2Int, Obstacle> spawnedObstacles = new();

    [SerializeField]
    private List<Obstacle> obstaclePrefabs;

    [Server]
    internal void SpawnObstacleOnMap(Dictionary<Vector2Int, Hex> grid, Vector2Int targetCoordinates, ObstacleType obstacleType)
    {
        Hex targetHex = Map.GetHex(grid, targetCoordinates);
        Obstacle obstaclePrefab = this.GetPrefabForType(obstacleType);
        GameObject obstacleObject = Instantiate(obstaclePrefab.gameObject, targetHex.transform.position, Quaternion.identity);
        obstacleObject.GetComponent<Obstacle>().hexPosition = targetHex.coordinates;
        NetworkServer.Spawn(obstacleObject);
        targetHex.SetObstacle(obstacleType);
        this.spawnedObstacles[targetCoordinates] = obstacleObject.GetComponent<Obstacle>();
    }

    [Server]
    public void DestroyObstacleAtPosition(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates)
    {
        Hex obstacleHex = Map.GetHex(grid, coordinates);
        if (!obstacleHex.HoldsAnObstacle())
        {
            throw new System.Exception("Trying to destroy obstacle at hex that doesn't contain one..");
        }

        Obstacle ObstacleToDestroy = this.spawnedObstacles[coordinates];
        this.spawnedObstacles.Remove(coordinates);
        Destroy(ObstacleToDestroy.gameObject);
        obstacleHex.ClearObstacle();
    }

    private Obstacle GetPrefabForType(ObstacleType type)
    {
        return this.obstaclePrefabs.Single<Obstacle>(obstacle => obstacle.type == type);
    }
}
