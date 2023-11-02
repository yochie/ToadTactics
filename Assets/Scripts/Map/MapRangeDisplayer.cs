using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRangeDisplayer : MonoBehaviour
{
    [SerializeField]
    private Map map;
    [SerializeField]
    private MapLOSDisplayer MapLOSDisplayer;

    private HashSet<Hex> displayedMoveRange = new();
    private Dictionary<Hex, LOSTargetType> displayedActionRange = new();
    private List<Hex> displayedPath = new();
    private List<Hex> highlightedArea = new();

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
        List<TargetType> allowedTargets = attacker.CurrentStats.allowedAttackTargets;
        bool requiresLOS = attacker.CurrentStats.attacksRequireLOS;
        Dictionary<Hex, LOSTargetType> attackRange = MapPathfinder.FindActionRange(this.map.hexGrid, source, range, allowedTargets, attacker, requiresLOS);
        this.displayedActionRange = attackRange;
        foreach (Hex h in attackRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (attackRange[h] == LOSTargetType.inRange)
                    h.drawer.DisplayInAttackRange(true);
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
                    h.drawer.DisplayInAbilityRange(true);
                else if (abilityRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayAbilityTargetable(true);
                else if (abilityRange[h] == LOSTargetType.outOfRange)
                {
                    h.drawer.DisplayOutOfActionRange(true);
                }
            }
        }
    }

    public void HideAttackRange()
    {
        foreach (Hex h in this.displayedActionRange.Keys)
        {
            if (this.displayedActionRange[h] == LOSTargetType.inRange)
                h.drawer.DisplayInAttackRange(false);
            else if (this.displayedActionRange[h] == LOSTargetType.targetable)
            {
                h.drawer.DisplayAttackTargetable(false);
            }                
            else if (this.displayedActionRange[h] == LOSTargetType.outOfRange)
            {
                h.drawer.DisplayOutOfActionRange(false);
            }
        }
    }

    internal void DisplayBallistaRange(Hex source, Ballista ballista, PlayerCharacter attacker)
    {
        List<TargetType> allowedTargets = ballista.allowedAttackTargets;
        bool requiresLOS = ballista.attacksRequireLOS;
        Dictionary<Hex, LOSTargetType> attackRange = MapPathfinder.FindActionRange(this.map.hexGrid, source, ballista.range, allowedTargets, attacker, requiresLOS);
        this.displayedActionRange = attackRange;
        foreach (Hex h in attackRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (attackRange[h] == LOSTargetType.inRange)
                    h.drawer.DisplayInAttackRange(true);
                else if (attackRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayAttackTargetable(true);
                else if (attackRange[h] == LOSTargetType.outOfRange)
                {
                    h.drawer.DisplayOutOfActionRange(true);
                }
            }
        }
    }

    public void HideBallistaRange()
    {
        foreach (Hex h in this.displayedActionRange.Keys)
        {
            if (this.displayedActionRange[h] == LOSTargetType.inRange)
                h.drawer.DisplayInAttackRange(false);
            else if (this.displayedActionRange[h] == LOSTargetType.targetable)
            {
                h.drawer.DisplayAttackTargetable(false);
            }
            else if (this.displayedActionRange[h] == LOSTargetType.outOfRange)
            {
                h.drawer.DisplayOutOfActionRange(false);
            }
        }
    }

    public void HideAbilityRange()
    {
        foreach (Hex h in this.displayedActionRange.Keys)
        {
            if (this.displayedActionRange[h] == LOSTargetType.inRange)
                h.drawer.DisplayInAbilityRange(false);
            else if (this.displayedActionRange[h] == LOSTargetType.targetable)
            {
                h.drawer.DisplayAbilityTargetable(false);
            }                
            else if (this.displayedActionRange[h] == LOSTargetType.outOfRange)
            {
                h.drawer.DisplayOutOfActionRange(false);
            }
        }
    }

    internal void UnHighlightTargetedArea()
    {
        foreach (Hex h in this.highlightedArea)
        {
            h.drawer.defaultHover(false);
        }
        this.MapLOSDisplayer.HideLOS();
    }

    internal void HighlightTargetedArea(Hex sourceHex, Hex primaryTargetHex, AreaType areaType, int areaScaler, bool requiresLOS)
    {
        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, areaType, sourceHex, primaryTargetHex, areaScaler);

        foreach (Hex targetedHex in targetedHexes)
        {
            this.highlightedArea.Add(targetedHex);
            targetedHex.drawer.defaultHover(true);
        }

        if (requiresLOS || areaType == AreaType.pierce)
            this.MapLOSDisplayer.DisplayLOS(source: sourceHex, destination: primaryTargetHex, highlightPath: false);
    }
}
