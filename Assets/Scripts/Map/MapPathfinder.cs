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

    internal static List<Hex> HexesOnLine(Hex startHex, Hex endHex)
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
            if (hitHex != null && hitHex != startHex)
            {
                //hitHex.DisplayLOSObstruction(true);
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

    public static HashSet<Hex> RangeWithObstaclesAndMoveCost(Hex start, int distance, Dictionary<Vector2Int, Hex> hexGrid)
    {
        HashSet<Hex> visited = new();
        visited.Add(start);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex> { start });
        Dictionary<Hex, int> costsSoFar = new();
        costsSoFar[start] = 0;

        for (int k = 1; k <= distance; k++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex h in fringes[k - 1])
            {
                foreach (Hex neighbour in MapPathfinder.HexUnobstructedNeighbours(h, hexGrid))
                {
                    int costToNeighbour = costsSoFar[h] + neighbour.MoveCost();
                    if (!costsSoFar.ContainsKey(neighbour) || costsSoFar[neighbour] > costToNeighbour)
                    {
                        if (costToNeighbour <= distance)
                        {
                            costsSoFar[neighbour] = costToNeighbour;
                            visited.Add(neighbour);
                            fringes[k].Add(neighbour);
                        }
                    }

                }
            }
        }

        return visited;
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

    //TODO : remove if no issues with using LOSReaches instead
    //private static bool IsHexReachableByLOS(Hex source, Hex target)
    //{
    //    LayerMask hexMask = LayerMask.GetMask("MapHex");
    //    RaycastHit2D[] hits;
    //    Vector2 sourcePos = source.transform.position;
    //    Vector2 targetPos = target.transform.position;
    //    Vector2 direction = targetPos - sourcePos;

    //    hits = Physics2D.RaycastAll(sourcePos, direction, direction.magnitude, hexMask);
    //    Array.Sort(hits, Comparer<RaycastHit2D>.Create((RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance)));

    //    bool firstHit = true;
    //    foreach (RaycastHit2D raycastHit in hits)
    //    {
    //        //skip first hit, its always source hex
    //        if (firstHit)
    //        {
    //            firstHit = false;
    //            continue;
    //        }

    //        Hex hitHex = raycastHit.collider.GetComponent<Hex>();
    //        if (hitHex != null && hitHex.BreaksLOSToTarget(target))
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}

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
}
