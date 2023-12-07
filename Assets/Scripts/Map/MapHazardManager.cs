using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class MapHazardManager : NetworkBehaviour
{
    //tracked on each client
    private Dictionary<Vector2Int, Hazard> spawnedHazardSprites = new();

    [Server]
    internal void SpawnHazardOnMap(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates, HazardType type, bool alsoDisplaySprite = true)
    {
        Hex hazardHex = Map.GetHex(grid, coordinates.x, coordinates.y);

        if (hazardHex.HoldsAHazard())
        {
            HazardType previousHazard = hazardHex.holdsHazard;
            //fire + apple = cooked apple
            if ((previousHazard == HazardType.apple && type == HazardType.fire) ||
                (type == HazardType.apple && previousHazard == HazardType.fire))
            {
                RemoveHazardAtPosition(grid, coordinates);
                type = HazardType.cookedApple;
            //fire + ice = none
            } else if ((previousHazard == HazardType.cold && type == HazardType.fire) ||
                (previousHazard == HazardType.fire && type == HazardType.cold))
            {
                RemoveHazardAtPosition(grid, coordinates);
                return;
            //by default, replace previous hazard 
            } else
            {
                RemoveHazardAtPosition(grid, coordinates);
            }
        }

        hazardHex.SetHazard(type);

        if(alsoDisplaySprite)
            this.RpcDisplayHazardSprite(coordinates, type);
    }

    [Server]
    public void RemoveHazardAtPosition(Dictionary<Vector2Int, Hex> grid, Vector2Int coordinates, bool alsoDestroySprite = true)
    {
        Hex hazardHex = Map.GetHex(grid, coordinates.x, coordinates.y);
        if(!hazardHex.HoldsAHazard())
        {
            Debug.Log("Trying to destroy hazard at hex that doesn't contain one. Probably an error somewhere.");
            return;
        }
        hazardHex.SetHazard(HazardType.none);

        if(alsoDestroySprite)
            RpcDestroyHazardSprite(coordinates);
    }

    [ClientRpc]
    public void RpcDisplayHazardSprite(Vector2Int coordinates, HazardType type)
    {
            AnimationSystem.Singleton.Queue(this.DisplayHazardCoroutine(coordinates, type));
    }

    private IEnumerator DisplayHazardCoroutine(Vector2Int coordinates, HazardType type)
    {
        if (this.spawnedHazardSprites.ContainsKey(coordinates))
        {
            Debug.Log("Warning: attempting to create hazard sprite at position that already contains one. Perhaps you forgot to remove it beforehand?");
            yield break;
        }

        Dictionary<Vector2Int, Hex> grid = Map.Singleton.hexGrid;
        Hex hazardHex = Map.GetHex(grid, coordinates.x, coordinates.y);

        Hazard hazardTypePrefab = HazardDataSO.Singleton.GetHazardPrefab(type).GetComponent<Hazard>();
        GameObject hazardObject = Instantiate(hazardTypePrefab.gameObject, hazardHex.transform.position, Quaternion.identity);
        this.spawnedHazardSprites[coordinates] = hazardObject.GetComponent<Hazard>();
        yield break;
    }

    [ClientRpc]
    public void RpcDestroyHazardSprite(Vector2Int coordinates)
    {
            AnimationSystem.Singleton.Queue(this.DestroyHazardSpriteCoroutine(coordinates));

    }

    private IEnumerator DestroyHazardSpriteCoroutine(Vector2Int coordinates)
    {
        if (!this.spawnedHazardSprites.ContainsKey(coordinates))
        {
            Debug.Log("Warning: attempting to destroy hazard sprite at position that has no record of it. Perhaps it was already destroyed?");
            yield break;
        }

        Hazard hazardToDestroy = this.spawnedHazardSprites[coordinates];
        Destroy(hazardToDestroy.gameObject);
        this.spawnedHazardSprites.Remove(coordinates);
        yield break;
    }

}
