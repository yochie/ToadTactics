using System;
using System.Collections.Generic;
using UnityEngine;

internal class AreaGenerator
{
    internal static List<Hex> GetHexesInArea(Dictionary<Vector2Int, Hex> hexGrid, AreaType areaType, Hex sourceHex, Hex primaryTargetHex, int scale)
    {
        List<Hex> hitHexes = new();
        switch (areaType) {
            case AreaType.single:
                hitHexes.Add(primaryTargetHex);
                break;
            case AreaType.radial:
                hitHexes.AddRange(MapPathfinder.RangeIgnoringObstacles(primaryTargetHex, scale, hexGrid));
                break;
            case AreaType.pierce:
                hitHexes.AddRange(MapPathfinder.HexesOnLine(sourceHex, primaryTargetHex));
                break;
            case AreaType.arc:
                hitHexes.AddRange(MapPathfinder.HexesInArc(sourceHex, primaryTargetHex, hexGrid, scale));
                break;
            case AreaType.ownTeam:
            case AreaType.enemyTeam:
                throw new Exception("Team area types should not use AreaGenerator.");
            default :
                hitHexes.Add(primaryTargetHex);
                break;
        }
        return hitHexes;
    }

    internal static List<Hex> GetHexesForTeam(Dictionary<Vector2Int, Hex> hexGrid,
                                              Mirror.SyncDictionary<int, HexCoordinates> characterPositions,
                                              Mirror.SyncDictionary<int, int> draftedCharacterOwners,
                                              AreaType abilityAreaType,
                                              Hex targetHex)
    {
        if (!targetHex.HoldsACharacter())
        {
            Debug.Log("Getting hexes for team of of char at hex that contains no char...0");
            return null;
        }

        if(abilityAreaType != AreaType.ownTeam && abilityAreaType != AreaType.enemyTeam)
        {
            throw new Exception("GetHexesForTeam is being used with invalid area type");
        }

        int ownPlayerID = targetHex.GetHeldCharacterObject().OwnerID;
        int teamToTarget = abilityAreaType == AreaType.ownTeam ? ownPlayerID : GameController.Singleton.OtherPlayer(ownPlayerID);
        List<Hex> teamHexes = new();
        foreach(var (charID, ownerID) in draftedCharacterOwners)
        {
            if (ownerID == teamToTarget)
                teamHexes.Add(Map.GetHex(hexGrid, characterPositions[charID]));
        }
        return teamHexes;
    }
}