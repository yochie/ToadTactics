using System;
using System.Collections.Generic;
using UnityEngine;

internal class AttackAreaGenerator
{
    internal static List<Hex> GetAttackArea(Dictionary<Vector2Int, Hex> hexGrid, AreaType attackAreaType, Hex attackerHex, Hex primaryTargetHex, int scale)
    {
        List<Hex> hitHexes = new();
        switch (attackAreaType) {
            case AreaType.single:
                hitHexes.Add(primaryTargetHex);
                break;
            case AreaType.radius:
                hitHexes.AddRange(MapPathfinder.RangeIgnoringObstacles(attackerHex, scale, hexGrid));
                break;
            case AreaType.pierce:
                hitHexes.AddRange(MapPathfinder.HexesOnLine(attackerHex, primaryTargetHex));
                break;
            //TODO: add arc
            default :
                hitHexes.Add(primaryTargetHex);
                break;
        }
        return hitHexes;
    }
}