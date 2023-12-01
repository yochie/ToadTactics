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
    internal void SpawnHazardOnMap(Dictionary<Vector2Int, Hex> grid, Vector2Int spawnedHazardCoordinate, HazardType spawnedHazardType)
    {
        Hex hazardHex = Map.GetHex(grid, spawnedHazardCoordinate.x, spawnedHazardCoordinate.y);


        if (hazardHex.HoldsAHazard())
        {
            HazardType previousHazard = hazardHex.holdsHazard;
            //fire + apple = cooked apple
            if ((previousHazard == HazardType.apple && spawnedHazardType == HazardType.fire) ||
                (spawnedHazardType == HazardType.apple && previousHazard == HazardType.fire))
            {
                DestroyHazardAtPosition(grid, spawnedHazardCoordinate);
                spawnedHazardType = HazardType.cookedApple;
            //fire + ice = none
            } else if ((previousHazard == HazardType.cold && spawnedHazardType == HazardType.fire) ||
                (previousHazard == HazardType.fire && spawnedHazardType == HazardType.cold))
            {
                DestroyHazardAtPosition(grid, spawnedHazardCoordinate);
                return;
            //by default, replace previous hazard 
            } else
            {
                DestroyHazardAtPosition(grid, spawnedHazardCoordinate);
            }
        }

        Hazard hazardTypePrefab = HazardDataSO.Singleton.GetHazardPrefab(spawnedHazardType).GetComponent<Hazard>();
        GameObject hazardObject = Instantiate(hazardTypePrefab.gameObject, hazardHex.transform.position, Quaternion.identity);
        NetworkServer.Spawn(hazardObject);
        hazardHex.holdsHazard = spawnedHazardType;
        this.spawnedHazards[spawnedHazardCoordinate] = hazardObject.GetComponent<Hazard>();
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
        NetworkServer.Destroy(hazardToDestroy.gameObject);
        this.spawnedHazards.Remove(coordinates);
        hazardHex.holdsHazard = HazardType.none;
    }
}
