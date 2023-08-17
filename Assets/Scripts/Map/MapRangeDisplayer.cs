using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRangeDisplayer : MonoBehaviour
{
    [SerializeField]
    private Map map;
    private HashSet<Hex> displayedMoveRange = new();
    private Dictionary<Hex, LOSTargetType> displayedActionRange = new();
    private List<Hex> displayedPath = new();
    private List<Hex> displayedAOE = new();

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
        List<TargetType> allowedTargets = attacker.charClass.stats.allowedAttackTargets;
        bool requiresLOS = attacker.CurrentStats.attacksRequireLOS;
        Dictionary<Hex, LOSTargetType> attackRange = MapPathfinder.FindActionRange(this.map.hexGrid, source, range, allowedTargets, attacker, requiresLOS);
        this.displayedActionRange = attackRange;
        foreach (Hex h in attackRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (attackRange[h] == LOSTargetType.inRange)
                    h.drawer.DisplayInActionRange(true);
                else if (attackRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayAttackTargetable(true);
                else if (attackRange[h] == LOSTargetType.outOfRange)
                {
                    h.drawer.DisplayOutOfActionRange(true);
                }
            }
        }
    }

    internal void DisplayAbilityRange(Hex source, CharacterAbilityStats abilityStats, PlayerCharacter user)
    {
        if (abilityStats.stringID == "")
        {
            Debug.Log("No ability to display");
            return;
        }
        List<TargetType> allowedTargets = abilityStats.allowedAbilityTargets;
        bool requiresLOS = abilityStats.requiresLOS;
        int range = abilityStats.range;
        Dictionary<Hex, LOSTargetType> abilityRange = MapPathfinder.FindActionRange(this.map.hexGrid, source, range, allowedTargets, user, requiresLOS);
        this.displayedActionRange = abilityRange;
        foreach (Hex h in abilityRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (abilityRange[h] == LOSTargetType.inRange)
                    h.drawer.DisplayInActionRange(true);
                else if (abilityRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayAbilityTargetable(true);
                else if (abilityRange[h] == LOSTargetType.outOfRange)
                {
                    h.drawer.DisplayOutOfActionRange(true);
                }
            }
        }
    }

    public void HideActionRange()
    {
        foreach (Hex h in this.displayedActionRange.Keys)
        {
            if (this.displayedActionRange[h] == LOSTargetType.inRange)
                h.drawer.DisplayInActionRange(false);
            else if (this.displayedActionRange[h] == LOSTargetType.targetable)
            {
                h.drawer.DisplayAbilityTargetable(false);
                h.drawer.DisplayAttackTargetable(false);
            }                
            else if (this.displayedActionRange[h] == LOSTargetType.outOfRange)
            {
                h.drawer.DisplayOutOfActionRange(false);
            }
        }
    }

    internal void HideAOE()
    {
        foreach (Hex h in this.displayedAOE)
        {
            h.drawer.AbilityHover(false);
        }
    }

    internal void DisplayAOE(Hex source, int aoe)
    {
        this.displayedAOE = MapPathfinder.RangeIgnoringObstacles(source, aoe,this.map.hexGrid);

        foreach (Hex h in this.displayedAOE)
        {
            h.drawer.AbilityHover(true);
        }
    }
}
