using System.Collections.Generic;
using Mirror;
using UnityEngine;

public interface ITargetedAction : IAction
{
    public Hex TargetHex { get; set; }

    public List<TargetType> AllowedTargetTypes { get; set; }

    public bool RequiresLOS { get; set; }

    public int Range { get; set; }

    //Should be called by all ServerValidate implementations on concrete actions
    [Server]
    public static bool ValidateTarget(ITargetedAction action)
    {
        //just some convenience renaming
        PlayerCharacter actor = action.ActorCharacter;
        Hex actorHex = action.ActorHex;
        Hex targetedHex = action.TargetHex;
        List<TargetType> allowedTargets = action.AllowedTargetTypes;
        int range = action.Range;

        if (targetedHex == null ||
            !ActionExecutor.IsValidTargetType(actor, targetedHex, allowedTargets) ||
            (MapPathfinder.HexDistance(actorHex, targetedHex) > range) ||
            (action.RequiresLOS && !MapPathfinder.LOSReaches(actorHex, targetedHex, range))
            )
        {
            Debug.Log("Target validation failed");
            return false;
        }
            
        return true;
    }
}
