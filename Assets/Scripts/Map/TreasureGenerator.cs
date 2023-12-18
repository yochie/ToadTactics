using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TreasureGenerator : NetworkBehaviour
{
    [SerializeField]
    private List<TreasureSpawnLocation> spawnLocations;
    
    [SerializeField]
    private List<HazardType> spawnedHazardTypes;
    
    [SerializeField]
    private GameObject treasurePrefab;

    [SerializeField]
    private GameObject ballistaPrefab;

    [SerializeField]
    private ObstacleType obstacleType;

    [SerializeField]
    private MapHazardManager hazardManager;

    [SerializeField]
    private MapObstacleManager obstacleManager;

    [SerializeField]
    private GameObject treasureParent;

    [Server]
    public void GenerateTreasureAndBallista(Dictionary<Vector2Int, Hex> grid)
    {
        //choose location at random
        TreasureSpawnLocation chosenSpawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        Hex ballistaHex = Map.GetHex(grid, chosenSpawnLocation.ballistaCoordinate.x, chosenSpawnLocation.ballistaCoordinate.y);
        GameObject ballistaObject = Instantiate(this.ballistaPrefab, ballistaHex.transform.position, Quaternion.identity, this.treasureParent.transform);
        NetworkServer.Spawn(ballistaObject);
        ballistaHex.holdsBallista = true;

        Hex treasureHex = Map.GetHex(grid, chosenSpawnLocation.treasureCoordinate.x, chosenSpawnLocation.treasureCoordinate.y);
        if (GameController.Singleton.CurrentRound < 2)
        {
            GameObject treasureObject = Instantiate(this.treasurePrefab, treasureHex.transform.position, Quaternion.identity, this.treasureParent.transform);
            NetworkServer.Spawn(treasureObject);
            treasureHex.SetTreasure(true);
            Map.Singleton.Treasure = treasureObject;
        }
        else
        {
            GameObject secondBallistaObject = Instantiate(this.ballistaPrefab, treasureHex.transform.position, Quaternion.identity, this.treasureParent.transform);
            NetworkServer.Spawn(secondBallistaObject);
            treasureHex.holdsBallista = true;
        }        

        //choose hazard type at random
        HazardType rolledHazardType = spawnedHazardTypes[Random.Range(0, spawnedHazardTypes.Count)];        

        foreach (Vector2Int hazardCoordinate in chosenSpawnLocation.hazardCoordinates)
        {
            this.hazardManager.SpawnHazardOnMap(grid, hazardCoordinate, rolledHazardType);
        }

        foreach (Vector2Int obstacleCoordinate in chosenSpawnLocation.obstacleCoordinates)
        {
            this.obstacleManager.SpawnObstacleOnMap(grid, obstacleCoordinate, this.obstacleType);
        }

    }
}
