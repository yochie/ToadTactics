using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TreasureSpawner : NetworkBehaviour
{
    public List<TreasureSpawnLocation> spawnLocations;
    public List<GameObject> hazardPrefabs;
    public GameObject treasurePrefab;

    public Map map;

    [Server]
    public void SpawnTreasure()
    {
        //choose location at random
        TreasureSpawnLocation randomLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        Hex treasureHex = this.map.GetHex(randomLocation.treasureCoordinate.x, randomLocation.treasureCoordinate.y);
        GameObject treasureObject = Instantiate(treasurePrefab, treasureHex.transform.position, Quaternion.identity);
        NetworkServer.Spawn(treasureObject);
        treasureHex.holdsTreasure = true;

        //choose hazard type at random
        GameObject randomHazardPrefab = hazardPrefabs[Random.Range(0, hazardPrefabs.Count)];
        Hazard randomHazard = randomHazardPrefab.GetComponent<Hazard>();

        foreach (Vector2Int hazardCoordinate in randomLocation.hazardCoordinates)
        {
            Hex hazardHex = this.map.GetHex(hazardCoordinate.x, hazardCoordinate.y);
            GameObject hazardObject = Instantiate(randomHazardPrefab, hazardHex.transform.position, Quaternion.identity);
            NetworkServer.Spawn(hazardObject);
            hazardHex.holdsHazard = randomHazard.type;
            if(randomHazard.type == HazardType.cold)
            {
                hazardHex.moveCost = 2;
            }
        }
    }
}
