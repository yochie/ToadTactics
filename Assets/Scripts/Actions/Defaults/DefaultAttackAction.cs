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
    public bool KnocksBack { get; set; }
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

    private void HitTarget(Hex target, INetworkedLogger logger)
    {
        if (target.HoldsAnObstacle())
        {
            //attacking obstacle
            Map.Singleton.obstacleManager.DestroyObstacleAtPosition(Map.Singleton.hexGrid, target.coordinates.OffsetCoordinatesAsVector());
            string logMessage = string.Format("{0} felled tree", this.ActorCharacter.charClass.name);
            logger.RpcLogMessage(logMessage);
            return;
        }

        if (!target.HoldsACharacter())
            return;

        PlayerCharacter DefenderCharacter = target.GetHeldCharacterObject();
        int prevLife = DefenderCharacter.CurrentLife;
        bool isCrit = Utility.RollCrit(this.CritChance);
        int critRolledDamage = isCrit ? Utility.CalculateCritDamage(this.Damage, this.CritMultiplier) : this.Damage;
        DefenderCharacter.TakeDamage(critRolledDamage, this.AttackDamageType, this.PenetratingDamage);

        string message = string.Format("{0} hit {1} for {2} ({6} {5}{7}) {3} => {4}",
        this.ActorCharacter.charClass.name,
        DefenderCharacter.charClass.name,
        critRolledDamage,
        prevLife,
        DefenderCharacter.CurrentLife,
        isCrit ? "crit" : "",
        this.AttackDamageType,
        this.PenetratingDamage ? " penetrating" : "");

        logger.RpcLogMessage(message);
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
}
