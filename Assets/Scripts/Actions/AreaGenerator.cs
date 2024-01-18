using System;
using System.Collections.Generic;
using UnityEngine;

internal class AreaGenerator
{
    internal static List<Hex> GetHexesInArea(Dictionary<Vector2Int, Hex> hexGrid, AreaType areaType, Hex sourceHex, Hex primaryTargetHex, int scale)
    {
        switch (areaType)
        {
            case AreaType.ownTeam:
            case AreaType.enemyTeam:
                return AreaGenerator.GetHexesForTeam(hexGrid, areaType, sourceHex);
            default:
                return AreaGenerator.GetHexesForGeometricArea(hexGrid, areaType, sourceHex,primaryTargetHex, scale);
        }
    }

    private static List<Hex> GetHexesForGeometricArea(Dictionary<Vector2Int, Hex> hexGrid, AreaType areaType, Hex sourceHex, Hex primaryTargetHex, int scale)
    {
        List<Hex> hitHexes = new();
        switch (areaType)
        {
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
            default:
                hitHexes.Add(primaryTargetHex);
                break;
        }
        return hitHexes;
    }

    private static List<Hex> GetHexesForTeam(Dictionary<Vector2Int, Hex> hexGrid,
                                              AreaType abilityAreaType,
                                              Hex sourceHex)
    {

        Mirror.SyncDictionary<int, HexCoordinates> characterPositions = Map.Singleton.characterPositions;
        Mirror.SyncDictionary<int, int>  draftedCharacterOwners = GameController.Singleton.DraftedCharacterOwners;
        if (!sourceHex.HoldsACharacter())
        {
            Debug.Log("Getting hexes for team of of char at hex that contains no char...0");
            return null;
        }

        if(abilityAreaType != AreaType.ownTeam && abilityAreaType != AreaType.enemyTeam)
        {
            throw new Exception("GetHexesForTeam is being used with invalid area type");
        }
        List<Hex> teamHexes = new();
        PlayerCharacter sourceCharacter = sourceHex.GetHeldCharacterObject();
        if (sourceCharacter == null)
        {
            Debug.LogFormat("Couldn't get hexes for team from {0} ({1}), it contains no character.", sourceHex, sourceHex.coordinates);
            return teamHexes;
        }
        int ownPlayerID = sourceCharacter.OwnerID;
        int teamToTarget = abilityAreaType == AreaType.ownTeam ? ownPlayerID : GameController.Singleton.OtherPlayer(ownPlayerID);

        foreach(var (charID, ownerID) in draftedCharacterOwners)
        {
            if (ownerID == teamToTarget)
                teamHexes.Add(Map.GetHex(hexGrid, characterPositions[charID]));
        }
        return teamHexes;
    }
}