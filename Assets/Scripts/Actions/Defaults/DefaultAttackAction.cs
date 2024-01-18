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
    public string DamageSourceName { get; set; }

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
            //string logMessage = string.Format("{0} felled tree{1}", this.ActorCharacter.charClass.name, gotAnApple ? " and it dropped an apple!":"");
            //logger.RpcLogMessage(logMessage);
            return;
        }

        if (!target.HoldsACharacter())
            return;

        PlayerCharacter defenderCharacter = target.GetHeldCharacterObject();
        bool isCrit = Utility.RollChance(this.CritChance);
        int critRolledDamage = isCrit ? Utility.CalculateCritDamage(this.Damage, this.CritMultiplier) : this.Damage;

        Action<int> logMessageWithDamage = new((int rawDamage) => {
            string message = string.Format("{0}'s <b>{1}</b> hit {2} for <color={6}><b>{3} {4}</b></color>{5}",
            this.ActorCharacter.charClass.name,
            this.DamageSourceName,
            defenderCharacter.charClass.name,
            rawDamage,
            this.AttackDamageType,
            isCrit ? " (crit)" : "",
            Utility.DamageTypeToColorName(this.AttackDamageType)
            );

            logger.RpcLogMessage(message);
        });

        defenderCharacter.TakeDamage(new Hit(critRolledDamage, this.AttackDamageType, HitSource.CharacterAttack, isCrit, this.PenetratingDamage), logMessageWithDamage);

        if (this.Knockback > 0 && !defenderCharacter.IsDead)
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

                //if(knockbackSuccess)
                //    logger.RpcLogMessage(string.Format("{0} knocked back {1}.", this.ActorCharacter.charClass.name, defenderCharacter.charClass.name));
                //else
                //    logger.RpcLogMessage(string.Format("{0} attempted to knockback {1} but it was blocked.", this.ActorCharacter.charClass.name, defenderCharacter.charClass.name));
            }
        }
    }

    [Server]
    private EffectOnCharacter PreviewHitTargetEffect(Hex target)
    {
        if (!target.HoldsACharacter())
            return EffectOnCharacter.None();

        PlayerCharacter defenderCharacter = target.GetHeldCharacterObject();
        int damage = defenderCharacter.CalculateDamageFromHit(new Hit(this.Damage, this.AttackDamageType, HitSource.CharacterAttack, isCrit : false, this.PenetratingDamage));

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
        if (!IAction.ValidateBasicAction(this))
            return false;
        
        if (!this.ActorCharacter.HasAvailableAttacks())
        {
            //Debug.Log("Attack validation failed : no available attacks.");
            return false;
        }

        if(!ITargetedAction.ValidateTarget(this))
            return false;

        return true;
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

