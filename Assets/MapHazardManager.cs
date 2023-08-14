using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class MapHazardManager : MonoBehaviour
{
    //Server only
    private Dictionary<Vector2Int, Hazard> spawnedHazards = new();

    [Server]
    internal void SpawnHazardOnMap(Dictionary<Vector2Int, Hex> grid, Vector2Int hazardCoordinate, HazardType hazardType)
    {
        Hex hazardHex = Map.GetHex(grid, hazardCoordinate.x, hazardCoordinate.y);
        Hazard hazardTypePrefab = HazardDataSO.Singleton.GetHazardPrefab(hazardType).GetComponent<Hazard>();
        GameObject hazardObject = Instantiate(hazardTypePrefab.gameObject, hazardHex.transform.position, Quaternion.identity);
        NetworkServer.Spawn(hazardObject);
        hazardHex.holdsHazard = hazardType;
        this.spawnedHazards[hazardCoordinate] = hazardObject.GetComponent<Hazard>();
    }

    [Server]
    public void DestroyHazardAtPosition(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates)
    {
        Hex hazardHex = Map.GetHex(grid, coordinates.x, coordinates.y);
        if(hazardHex.holdsHazard == HazardType.none)
        {
            Debug.Log("Trying to destroy hazard at hex that doesn't contain one. Probably an error somewhere.");
            return;
        }

        Hazard hazardToDestroy = this.spawnedHazards[coordinates];
        this.spawnedHazards.Remove(coordinates);
        Destroy(hazardToDestroy.gameObject);
        hazardHex.holdsHazard = HazardType.none;
    }
}
