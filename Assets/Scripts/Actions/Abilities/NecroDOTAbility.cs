using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class NecroDOTAbility : IAbilityAction, IBuffSource, ITargetedAction, ICooldownedAction
{
    //IAction
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    //IAbilityAction
    public CharacterAbilityStats AbilityStats { get; set; }

    //IBuffSource
    public Type AppliesBuffType { get => typeof(NecroDOTEffect); }
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    //IAreaTargeter
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} using {1}", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);

        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        //self harm attack
        ActionExecutor.Singleton.CustomAttack(source: this.ActorHex,
                                              primaryTarget: this.ActorHex,
                                              areaType: AreaType.single,
                                              areaScaler: 1,
                                              damage: this.AbilityStats.damage,
                                              damageType: this.AbilityStats.damageType,
                                              damageIterations: this.AbilityStats.damageIterations,
                                              penetratingDamage: this.AbilityStats.penetratingDamage,
                                              knocksBack: false,
                                              canCrit: false,
                                              critChance: 0f,
                                              critMultiplier: 0f,
                                              sender: this.RequestingClient);

        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);

        List<int> affectedCharacterIDs = targetedHexes.Where(hex => hex.HoldsACharacter()).Select(hex => hex.holdsCharacterWithClassID).ToList();
        IAbilityBuffEffect buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffType, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);
        BuffManager.Singleton.ApplyNewBuff(buff);
    }

    [Server]
    public bool ServerValidate()
    {

        //TODO check for individual ability uses instead of single hasUsedAbility to allow multiple abilities

        if (IAction.ValidateBasicAction(this) &&
            ITargetedAction.ValidateTarget(this) &&
            IAbilityAction.ValidateCooldowns(this)
            )
            return true;
        else
            return false;
    }
}