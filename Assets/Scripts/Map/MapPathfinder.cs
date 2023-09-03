using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public static class MapPathfinder
{
    public static int HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (int)((Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f);
    }

    public static List<Hex> HexNeighbours(Hex h, Dictionary<Vector2Int, Hex> hexGrid)
    {
        List<Hex> toReturn = new();
        foreach (HexCoordinates neighbourCoord in h.coordinates.NeighbhouringCoordinates())
        {
            Hex neighbour = Map.GetHex(hexGrid, neighbourCoord.X, neighbourCoord.Y);
            if (neighbour != null)
            {
                toReturn.Add(neighbour);
            }
        }

        return toReturn;
    }

    //only supports range of 1
    internal static HashSet<Hex> HexesInArc(Hex sourceHex, Hex primaryTargetHex, Dictionary<Vector2Int, Hex> hexGrid, int scale)
    {
        if (MapPathfinder.HexDistance(sourceHex, primaryTargetHex) != 1)
            return new HashSet<Hex>() { primaryTargetHex };

        HexCoordinates sourceToTarget = HexCoordinates.Substract(primaryTargetHex.coordinates, sourceHex.coordinates);
        List<bool> directions = new() { true, false };
        HashSet<Hex> hexesInArc = new();
        hexesInArc.Add(primaryTargetHex);
        foreach (bool direction in directions)
        {
            HexCoordinates currentRotatedVector = sourceToTarget;
            for(int i = 0; i < scale; i++)
            {
                currentRotatedVector = HexCoordinates.RotateVector(currentRotatedVector, clockwise: direction);
                HexCoordinates hexCoordinatesAtRotation = HexCoordinates.Add(sourceHex.coordinates, currentRotatedVector);
                Hex hexAtRotation = Map.GetHex(hexGrid, hexCoordinatesAtRotation);
                if (hexAtRotation != null)
                    hexesInArc.Add(hexAtRotation);
            }
        }

        return hexesInArc;

    }

    internal static List<Hex> HexesOnLine(Hex startHex, Hex endHex, bool excludeStart = true, bool excludeDestination = false)
    {
        LayerMask hexMask = LayerMask.GetMask("MapHex");

        RaycastHit2D[] hits;
        Vector2 sourcePos = startHex.transform.position;
        Vector2 destPos = endHex.transform.position;
        Vector2 direction = destPos - sourcePos;
        List<Hex> hexesOnLine = new();

        hits = Physics2D.RaycastAll(sourcePos, direction, direction.magnitude, hexMask);
        foreach (RaycastHit2D hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            Hex hitHex = hitObject.GetComponent<Hex>();
            if (hitHex != null)
            {
                if (excludeStart && hitHex == startHex)
                    continue;
                if (excludeDestination && hitHex == endHex)
                    continue;
                hexesOnLine.Add(hitHex);
            }
        }
        return hexesOnLine;
    }

    //returns hexes without hazards, obstacles or players
    public static List<Hex> HexUnobstructedNeighbours(Hex h, Dictionary<Vector2Int, Hex> hexGrid)
    {
        List<Hex> toReturn = new();
        foreach (HexCoordinates neighbourCoord in h.coordinates.NeighbhouringCoordinates())
        {
            Hex neighbour = Map.GetHex(hexGrid, neighbourCoord.X, neighbourCoord.Y);
            if (neighbour != null && !neighbour.HoldsAnObstacle() && !neighbour.HoldsACharacter())
            {
                toReturn.Add(neighbour);
            }
        }

        return toReturn;
    }

    public static List<Hex> RangeIgnoringObstacles(Hex start, int distance, Dictionary<Vector2Int, Hex> hexGrid)
    {
        List<Hex> toReturn = new();

        for (int q = -distance; q <= distance; q++)
        {
            for (int r = Mathf.Max(-distance, -distance - q); r <= Mathf.Min(distance, -q + distance); r++)
            {
                HexCoordinates destCoords = HexCoordinates.Add(start.coordinates, new HexCoordinates(q, r, start.coordinates.isFlatTop));
                Hex destHex = Map.GetHex(hexGrid, destCoords.X, destCoords.Y);
                if (destHex != null)
                    toReturn.Add(destHex);
            }
        }
        //Debug.Log(toReturn);
        //Debug.Log(toReturn.Count);
        return toReturn;
    }

    public static HashSet<Hex> RangeWithObstacles(Hex start, int distance, Dictionary<Vector2Int, Hex> hexGrid)
    {
        HashSet<Hex> visited = new();
        visited.Add(start);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex> { start });

        for (int k = 1; k <= distance; k++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex h in fringes[k - 1])
            {
                foreach (Hex neighbour in MapPathfinder.HexUnobstructedNeighbours(h, hexGrid))
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        fringes[k - 1 + neighbour.MoveCost()].Add(neighbour);
                    }

                }
            }
        }

        return visited;
    }

    public static HashSet<Hex> RangeWithObstaclesAndMoveCost(Hex start, int maxDistance, Dictionary<Vector2Int, Hex> hexGrid)
    {
        HashSet<Hex> inRange = new();
        inRange.Add(start);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex> { start });
        Dictionary<Hex, int> costsSoFar = new();
        costsSoFar[start] = 0;

        for (int distance = 1; distance <= maxDistance; distance++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex fringeHex in fringes[distance - 1])
            {
                foreach (Hex neighbour in MapPathfinder.HexUnobstructedNeighbours(fringeHex, hexGrid))
                {
                    int costToNeighbour = costsSoFar[fringeHex] + neighbour.MoveCost();
                    if (!costsSoFar.ContainsKey(neighbour) || costsSoFar[neighbour] > costToNeighbour)
                    {
                        if (costToNeighbour <= maxDistance)
                        {
                            costsSoFar[neighbour] = costToNeighbour;
                            fringes[distance].Add(neighbour);
                            //prevents finishing moves on corpses
                            if (neighbour.HoldsACorpse() && costToNeighbour == maxDistance)
                                continue;
                            else
                                inRange.Add(neighbour);
                        }
                    }

                }
            }
        }

        return inRange;
    }

    //Returns null if destination is not in path
    public static List<Hex> FindMovementPath(Hex start, Hex dest, Dictionary<Vector2Int, Hex> hexGrid)
    {
        PriorityQueue<Hex, int> frontier = new();
        frontier.Enqueue(start, 0);

        Dictionary<Hex, Hex> cameFrom = new();
        cameFrom[start] = null;

        Dictionary<Hex, int> costsSoFar = new();
        costsSoFar[start] = 0;

        while (frontier.Count != 0)
        {
            Hex currentHex = frontier.Dequeue();

            if (currentHex == dest)
            {
                break;
            }

            foreach (Hex next in MapPathfinder.HexUnobstructedNeighbours(currentHex, hexGrid))
            {
                int newCost = costsSoFar[currentHex] + next.MoveCost();
                if (!costsSoFar.ContainsKey(next) || newCost < costsSoFar[next])
                {
                    costsSoFar[next] = newCost;
                    int priority = newCost + MapPathfinder.HexDistance(next, dest);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = currentHex;
                }
            }
        }
        List<Hex> toReturn = MapPathfinder.FlattenPath(cameFrom, dest);
        return toReturn;
    }

    public static Dictionary<Hex, LOSTargetType> FindActionRange(Dictionary<Vector2Int, Hex> hexGrid,
                                                                 Hex source,
                                                                 int range,
                                                                 List<TargetType> allowedTargetTypes,
                                                                 PlayerCharacter actor,
                                                                 bool requiresLOS)
    {
        Dictionary<Hex, LOSTargetType> hexTargetableTypes = new();
        List<Hex> allHexesInRange = MapPathfinder.RangeIgnoringObstacles(source, range, hexGrid);

        allHexesInRange.Sort(Comparer<Hex>.Create((Hex h1, Hex h2) => MapPathfinder.HexDistance(source, h1).CompareTo(MapPathfinder.HexDistance(source, h2))));
        foreach (Hex targetHex in allHexesInRange)
        {

            bool unobstructed;
            if (requiresLOS)
                unobstructed = MapPathfinder.LOSReaches(source, targetHex);
            else
                unobstructed = true;

            if (unobstructed)
            {
                if (ActionExecutor.IsValidTargetType(actor, targetHex, allowedTargetTypes))
                    hexTargetableTypes[targetHex] = LOSTargetType.targetable;
                else
                    hexTargetableTypes[targetHex] = LOSTargetType.inRange;
            }
            else
                hexTargetableTypes[targetHex] = LOSTargetType.outOfRange;

            if (unobstructed)
                if (!hexTargetableTypes.ContainsKey(targetHex) || (hexTargetableTypes.ContainsKey(targetHex) && hexTargetableTypes[targetHex] != LOSTargetType.targetable))
                    hexTargetableTypes[targetHex] = LOSTargetType.inRange;
        }
        return hexTargetableTypes;
    }
    public static bool LOSReaches(Hex source, Hex target, int? range = null)
    {

        LayerMask hexMask = LayerMask.GetMask("MapHex");

        if (range != null && MapPathfinder.HexDistance(source, target) > range.GetValueOrDefault())
            return false;

        bool unobstructed = true;
        RaycastHit2D[] hits;
        Vector2 sourcePos = source.transform.position;
        Vector2 destPos = target.transform.position;
        Vector2 direction = destPos - sourcePos;

        hits = Physics2D.RaycastAll(sourcePos, direction, direction.magnitude, hexMask);
        foreach (RaycastHit2D hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            Hex hitHex = hitObject.GetComponent<Hex>();
            if (hitHex != null && hitHex != source && hitHex.BreaksLOSToTarget(target))
            {
                //hitHex.DisplayLOSObstruction(true);
                unobstructed = false;
            }
        }
        return unobstructed;
    }

    public static int PathCost(List<Hex> path)
    {
        int pathCost = 0;
        if (path == null)
            return pathCost;
        foreach (Hex h in path)
        {
            pathCost += h.MoveCost();
        }

        return pathCost;
    }

    //Returns null if destination is not in path
    public static List<Hex> FlattenPath(Dictionary<Hex, Hex> path, Hex dest)
    {
        //no path to destination was found
        if (!path.ContainsKey(dest))
        {
            return null;
        }

        List<Hex> toReturn = new();
        Hex currentHex = dest;
        while (path[currentHex] != null)
        {
            toReturn.Add(currentHex);
            currentHex = path[currentHex];
        }
        toReturn.Reverse();
        return toReturn;
    }

    public static Hex KnockbackAlongAxis(Dictionary<Vector2Int, Hex> hexGrid, Hex source, Hex target, int knockbackDistance)
    {
        HexCoordinates difference = HexCoordinates.Substract(target.coordinates, source.coordinates);

        if (difference.OnSingleAxis())
            throw new Exception("Knoback only supported along single axis");

        HexCoordinates knockbackVector = difference.Elongate(knockbackDistance);

        HexCoordinates destination = target.coordinates.Add(knockbackVector);
        return Map.GetHex(hexGrid, destination);
    }
}
