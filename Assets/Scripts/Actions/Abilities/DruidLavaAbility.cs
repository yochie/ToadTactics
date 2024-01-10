using Mirror;
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

    //IAreaTargeter
    public AreaType TargetedAreaType {get; set;}
    public int AreaScaler { get; set; }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} used <b>{1}</b>", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);

        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);

        foreach (Hex hex in targetedHexes)
        { 
            if (hex.holdsHazard == HazardType.fire)
            {
                continue;
            }

            MapHazardManager hazardManager = Map.Singleton.hazardManager;
            if (hex.holdsHazard == HazardType.cold)
            {
                hazardManager.RemoveHazardAtPosition(Map.Singleton.hexGrid, hex.coordinates.OffsetCoordinatesAsVector());
            }

            hazardManager.SpawnHazardOnMap(Map.Singleton.hexGrid, hex.coordinates.OffsetCoordinatesAsVector(), HazardType.fire);
        }

        //apply hazard damage to any already present character and destroy obstacles
        //ability needs to be configured manually with hazard damage.... not great but i think ok
        ActionExecutor.Singleton.AbilityAttack(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);
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

    public ActionEffectPreview PreviewEffect()
    {
        ActionEffectPreview baseEffectPreview = ActionEffectPreview.None();
        ActionEffectPreview attackPortionPreview = ActionExecutor.Singleton.GetAbilityAttackPreview(this.ActorHex, this.TargetHex, this.AbilityStats, this.RequestingClient);

        return baseEffectPreview.MergeWithPreview(attackPortionPreview);
    }
}