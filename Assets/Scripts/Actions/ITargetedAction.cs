using System.Collections.Generic;
using Mirror;

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

        if(!ActionExecutor.IsValidTargetType(actor, targetedHex, allowedTargets))
            return false;

        if (MapPathfinder.HexDistance(actorHex, targetedHex) > range)
            return false;

        if (action.RequiresLOS && !MapPathfinder.LOSReaches(actorHex, targetedHex, range))
            return false;
        
        return true;
    }
}
