using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class DefaultAttackAction : IAttackAction
{
    //IAction
    public PlayerCharacter ActorCharacter { get; set; }
    public Hex ActorHex { get; set; }
    public int RequestingPlayerID { get; set; }
    public NetworkConnectionToClient RequestingClient { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    //IAttackAction
    public int Damage { get; set; }
    public int DamageIterations { get; set; }
    public DamageType AttackDamageType { get; set; }
    public bool PenetratingDamage { get; set; }
    public int Knockback { get; set; }
    public float CritChance { get; set; }
    public float CritMultiplier { get; set; }
    public AreaType TargetedAreaType { get; set; }
    public int AreaScaler { get; set; }

    [Server]
    public virtual void ServerUse(INetworkedLogger logger)
    {
        List<Hex> allTargets = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);
        for (int i = 0; i < this.DamageIterations; i++)
        {
            foreach (Hex target in allTargets)
            {
                this.HitTarget(target, logger);
            }
        }

        //PlayerCharacter state updated to track that attack was used
        this.ActorCharacter.UsedAttack();        
    }

    [Server]
    protected void HitTarget(Hex target, INetworkedLogger logger)
    {
        if (target.HoldsAnObstacle())
        {
            //attacking obstacle
            Map.Singleton.obstacleManager.RemoveObstacleAtPosition(Map.Singleton.hexGrid, target.coordinates.OffsetCoordinatesAsVector());
            bool gotAnApple = Utility.RollChance(Map.Singleton.appleSpawnChance);
            if (gotAnApple)
                Map.Singleton.hazardManager.SpawnHazardOnMap(Map.Singleton.hexGrid, target.coordinates.OffsetCoordinatesAsVector(), HazardType.apple);
            string logMessage = string.Format("{0} felled tree{1}", this.ActorCharacter.charClass.name, gotAnApple ? " and it dropped an apple!":"");
            logger.RpcLogMessage(logMessage);
            return;
        }

        if (!target.HoldsACharacter())
            return;

        PlayerCharacter defenderCharacter = target.GetHeldCharacterObject();
        int prevLife = defenderCharacter.CurrentLife;
        bool isCrit = Utility.RollChance(this.CritChance);
        int critRolledDamage = isCrit ? Utility.CalculateCritDamage(this.Damage, this.CritMultiplier) : this.Damage;
        defenderCharacter.TakeDamage(new Hit(critRolledDamage, this.AttackDamageType, HitSource.CharacterAttack, this.PenetratingDamage));

        if (this.Knockback > 0)
        {
            HexCoordinates sourceToTarget = HexCoordinates.Substract(target.coordinates, this.ActorHex.coordinates);
            if (sourceToTarget.OnSingleAxis())
            {                
                Hex knockbackDestination = MapPathfinder.KnockbackAlongAxis(Map.Singleton.hexGrid, this.ActorHex, target, knockbackDistance: Knockback);
                bool knockbackSuccess;
                if (knockbackDestination != null)
                    knockbackSuccess = ActionExecutor.Singleton.CustomMove(target, knockbackDestination, this.RequestingClient);
                else
                    knockbackSuccess = false;

                if(knockbackSuccess)
                    logger.RpcLogMessage(string.Format("{0} knocked back {1}.", this.ActorCharacter.charClass.name, defenderCharacter.charClass.name));
                else
                    logger.RpcLogMessage(string.Format("{0} attempted to knockback {1} but it was blocked.", this.ActorCharacter.charClass.name, defenderCharacter.charClass.name));
            }
        }

        string message = string.Format("{0} hit {1} for {2} ({6} {5}{7}) {3} => {4}",
        this.ActorCharacter.charClass.name,
        defenderCharacter.charClass.name,
        critRolledDamage,
        prevLife,
        defenderCharacter.CurrentLife,
        isCrit ? "crit" : "",
        this.AttackDamageType,
        this.PenetratingDamage ? " penetrating" : "");

        logger.RpcLogMessage(message);
    }

    [Server]
    private EffectOnCharacter PreviewHitTargetEffect(Hex target)
    {
        if (!target.HoldsACharacter())
            return EffectOnCharacter.None();

        PlayerCharacter defenderCharacter = target.GetHeldCharacterObject();
        int damage = defenderCharacter.CalculateDamageFromHit(new Hit(this.Damage, this.AttackDamageType, HitSource.CharacterAttack, this.PenetratingDamage));

        EffectOnCharacter effect = new(defenderCharacter.CharClassID, target.coordinates, damage, damage);
        return effect;

        //TODO : enable knockback previews
        //if (this.Knockback > 0)
        //{
        //    HexCoordinates sourceToTarget = HexCoordinates.Substract(target.coordinates, this.ActorHex.coordinates);
        //    if (sourceToTarget.OnSingleAxis())
        //    {
        //        Hex knockbackDestination = MapPathfinder.KnockbackAlongAxis(Map.Singleton.hexGrid, this.ActorHex, target, knockbackDistance: Knockback);


        //        bool knockbackSuccess;
        //        if (knockbackDestination != null)
        //            knockbackSuccess = ActionExecutor.Singleton.CustomMove(target, knockbackDestination, this.RequestingClient);
        //        else
        //            knockbackSuccess = false;               
        //    }
        //}
    }


    [Server]
    public virtual bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            this.ActorCharacter.HasAvailableAttacks() &&
            ITargetedAction.ValidateTarget(this)
            )
            return true;
        else
            return false;
    }

    [Server]
    public virtual ActionEffectPreview PreviewEffect()
    {
        ActionEffectPreview effectPreview = ActionEffectPreview.None();

        List<Hex> allTargets = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, this.TargetedAreaType, this.ActorHex, this.TargetHex, this.AreaScaler);
       
        for (int i = 0; i < this.DamageIterations; i++)
        {
            foreach (Hex target in allTargets)
            {
                EffectOnCharacter effectOnCharacter = this.PreviewHitTargetEffect(target);
                if(effectOnCharacter.classID != -1)
                    effectPreview = effectPreview.AddEffect(effectOnCharacter);
            }
        }

        return effectPreview;
    }
}

