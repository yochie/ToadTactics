using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WarriorRootAbility : IAbilityAction, ITargetedAction, IActivatedBuffSource
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
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }
    
    //IBuffSource
    public IBuffDataSO AppliesBuffOnActivation { get => BuffDataSO.Singleton.GetBuffData("WarriorRootBuff"); }

    [Server]
    public void ServerUse(INetworkedLogger logger)
    {
        string message = string.Format("{0} used <b>{1}</b>", this.ActorCharacter.charClass.name, this.AbilityStats.interfaceName);
        logger.RpcLogMessage(message);

        this.ActorCharacter.UsedAbility(this.AbilityStats.stringID);

        List<Hex> targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);

        //filter out friendly characters
        List<PlayerCharacter> affectedCharacters = targetedHexes
            .Where(hex => hex.HoldsACharacter() && hex.GetHeldCharacterObject().OwnerID != this.RequestingPlayerID)
            .Select(hex => hex.GetHeldCharacterObject()).ToList();
        List<int> affectedCharacterIDs = affectedCharacters.Select(character => character.CharClassID).ToList();
        RuntimeBuff buff = BuffManager.Singleton.CreateAbilityBuff(this.AppliesBuffOnActivation, this.AbilityStats, this.ActorCharacter.CharClassID, affectedCharacterIDs);
        BuffManager.Singleton.ApplyNewBuff(buff);

        foreach (Hex targetedHex in targetedHexes)
        {
            if (!targetedHex.HoldsACharacter() || targetedHex.GetHeldCharacterObject().OwnerID == this.RequestingPlayerID)
                continue;

            PlayerCharacter affectedCharacter = targetedHex.GetHeldCharacterObject();
            if (affectedCharacter == null)
                continue;

            if (this.AbilityStats.knockback > 0 && !affectedCharacter.IsDead)
            {
                HexCoordinates sourceToTarget = HexCoordinates.Substract(targetedHex.coordinates, this.ActorHex.coordinates);
                if (!sourceToTarget.OnSingleAxis())
                    continue;

                Hex knockbackDestination = MapPathfinder.KnockbackAlongAxis(Map.Singleton.hexGrid, this.ActorHex, targetedHex, knockbackDistance: this.AbilityStats.knockback);
                bool knockbackSuccess;
                if (knockbackDestination != null)
                    knockbackSuccess = ActionExecutor.Singleton.CustomMove(targetedHex, knockbackDestination, this.RequestingClient);
                else
                    knockbackSuccess = false;

                string knockbackMessage;
                if (knockbackSuccess)
                {
                    knockbackMessage = string.Format("{0}'s <b>{1}</b> <color={3}><b>knockbacked</b></color> {2}",
                        this.ActorCharacter.charClass.name,
                        this.AbilityStats.interfaceName,
                        affectedCharacter.charClass.name,
                        Utility.DamageTypeToColorName(DamageType.physical));
                }
                else
                {
                    knockbackMessage = string.Format("{0}'s <b>{1}</b> attempted to knockback {2} but was blocked",
                        this.ActorCharacter.charClass.name,
                        this.AbilityStats.interfaceName,
                        affectedCharacter.charClass.name);
                }
                logger.RpcLogMessage(knockbackMessage);
            }
        }
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

    public ActionEffectPreview PreviewEffect()
    {

        return ActionEffectPreview.None();
    }
}