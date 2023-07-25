using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRangeDisplayer : MonoBehaviour
{
    [SerializeField]
    private Map map;
    private HashSet<Hex> displayedMoveRange = new();
    private Dictionary<Hex, LOSTargetType> displayedAttackRange = new();
    private List<Hex> displayedPath = new();

    public void DisplayMovementRange(Hex source, int moveDistance)
    {
        this.displayedMoveRange = MapPathfinder.RangeWithObstaclesAndMoveCost(source, moveDistance, this.map.hexGrid);
        foreach (Hex h in this.displayedMoveRange)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                h.drawer.DisplayInMoveRange(true);
            }
        }
    }

    public void HideMovementRange()
    {
        foreach (Hex h in this.displayedMoveRange)
        {
            h.drawer.DisplayInMoveRange(false);
        }
    }

    public void DisplayPath(List<Hex> path)
    {
        //save path for hiding later
        this.displayedPath = path;
        int pathLength = 0;
        foreach (Hex h in path)
        {
            pathLength += h.MoveCost();

            //skip starting hex label
            if (pathLength != 0)
            {
                h.drawer.LabelString = pathLength.ToString();
                h.drawer.ShowLabel();
            }
        }
    }

    public void HidePath()
    {
        foreach (Hex h in this.displayedPath)
        {
            h.drawer.HideLabel();
        }
    }

    public void DisplayAttackRange(Hex source, int range, PlayerCharacter attacker)
    {
        Dictionary<Hex, LOSTargetType> attackRange = MapPathfinder.FindAttackRange(source, range, this.map.hexGrid, attacker.charClass.stats.allowedAttackTargets, attacker);
        this.displayedAttackRange = attackRange;
        foreach (Hex h in attackRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (attackRange[h] == LOSTargetType.inRange)
                    h.drawer.DisplayInActionRange(true);
                else if (attackRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayTargetable(true);
                else if (attackRange[h] == LOSTargetType.outOfRange)
                {
                    h.drawer.DisplayOutOfActionRange(true);
                }
            }
        }
    }

    public void HideAttackRange()
    {
        foreach (Hex h in this.displayedAttackRange.Keys)
        {
            if (this.displayedAttackRange[h] == LOSTargetType.inRange)
                h.drawer.DisplayInActionRange(false);
            else if (this.displayedAttackRange[h] == LOSTargetType.targetable)
                h.drawer.DisplayTargetable(false);
            else if (this.displayedAttackRange[h] == LOSTargetType.outOfRange)
            {
                h.drawer.DisplayOutOfActionRange(false);
            }
        }
    }
}
