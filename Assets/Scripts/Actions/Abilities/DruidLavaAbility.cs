﻿using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DruidLavaAbility : IAbilityAction, ITargetedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    [Server]
    public void ServerUse()
    {
        Debug.Log("Using druid lava!");
        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> hexesInAOE = MapPathfinder.RangeIgnoringObstacles(this.TargetHex, this.AbilityStats.aoe, Map.Singleton.hexGrid);
        GameObject fireHazardPrefab = HazardDataSO.Singleton.GetHazardPrefab(HazardType.fire);

        foreach (Hex hex in hexesInAOE)
        {
            //apply hazard damage to any already present character and destroy obstacles
            //ability needs to be configured manually with hazard damage.... not great but i think ok
            if (hex.HoldsACharacter() || hex.HoldsAnObstacle())
                ActionExecutor.Singleton.AbilityAttack(this.ActorHex, hex, this.AbilityStats, this.RequestingClient);

            GameObject hazardObject = UnityEngine.Object.Instantiate(fireHazardPrefab, hex.transform.position, Quaternion.identity);
            NetworkServer.Spawn(hazardObject);
            hex.holdsHazard = HazardType.fire;
        }
    }

    [Server]
    public bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this) &&
            IAbilityAction.ValidateCooldowns(this)
            )
            return true;
        else
            return false;
    }
}