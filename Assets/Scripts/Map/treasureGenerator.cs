using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class treasureGenerator : NetworkBehaviour
{
    [SerializeField]
    private List<TreasureSpawnLocation> spawnLocations;
    [SerializeField]
    private List<GameObject> hazardPrefabs;
    [SerializeField]
    private GameObject treasurePrefab;


    //TODO : convert hazard prefab ref to use HazardData SO
    [Server]
    public void GenerateTreasure(Dictionary<Vector2Int, Hex> grid)
    {
        //choose location at random
        TreasureSpawnLocation randomLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        Hex treasureHex = Map.GetHex(grid, randomLocation.treasureCoordinate.x, randomLocation.treasureCoordinate.y);
        GameObject treasureObject = Instantiate(this.treasurePrefab, treasureHex.transform.position, Quaternion.identity);
        NetworkServer.Spawn(treasureObject);
        treasureHex.holdsTreasure = true;

        //choose hazard type at random
        GameObject randomHazardPrefab = hazardPrefabs[Random.Range(0, hazardPrefabs.Count)];
        Hazard randomHazard = randomHazardPrefab.GetComponent<Hazard>();

        foreach (Vector2Int hazardCoordinate in randomLocation.hazardCoordinates)
        {
            Hex hazardHex = Map.GetHex(grid, hazardCoordinate.x, hazardCoordinate.y);
            GameObject hazardObject = Instantiate(randomHazardPrefab, hazardHex.transform.position, Quaternion.identity);
            NetworkServer.Spawn(hazardObject);
            hazardHex.holdsHazard = randomHazard.Type;
        }
    }
}
