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
            default :
                hitHexes.Add(primaryTargetHex);
                break;
        }
        return hitHexes;
    }
}