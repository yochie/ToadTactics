using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class treasureGenerator : NetworkBehaviour
{
    [SerializeField]
    private List<TreasureSpawnLocation> spawnLocations;
    [SerializeField]
    private List<HazardType> spawnedHazardTypes;
    [SerializeField]
    private GameObject treasurePrefab;
    [SerializeField]
    private MapHazardManager hazardManager;

    //TODO : convert hazard prefab ref to use HazardData SO
    [Server]
    public void GenerateTreasure(Dictionary<Vector2Int, Hex> grid)
    {
        //choose location at random
        TreasureSpawnLocation chosenSpawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        Hex treasureHex = Map.GetHex(grid, chosenSpawnLocation.treasureCoordinate.x, chosenSpawnLocation.treasureCoordinate.y);
        GameObject treasureObject = Instantiate(this.treasurePrefab, treasureHex.transform.position, Quaternion.identity);
        NetworkServer.Spawn(treasureObject);
        treasureHex.holdsTreasure = true;

        //choose hazard type at random
        HazardType rolledHazardType = spawnedHazardTypes[Random.Range(0, spawnedHazardTypes.Count)];        

        foreach (Vector2Int hazardCoordinate in chosenSpawnLocation.hazardCoordinates)
        {
            this.hazardManager.SpawnHazardOnMap(grid, hazardCoordinate, rolledHazardType);
        }

        Map.Singleton.Treasure = treasureObject;
    }
}
