using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DefaultMoveAction : IMoveAction
{
    //IAction
    public int RequestingPlayerID { get; set; }
    public PlayerCharacter ActorCharacter { get; set; }
    public Hex ActorHex { get; set; }
    public NetworkConnectionToClient RequestingClient { get; set; }

    //ITargetedAction
    public Hex TargetHex { get; set; }
    public List<TargetType> AllowedTargetTypes { get; set; }
    public bool RequiresLOS { get; set; }
    public int Range { get; set; }

    //IMoveAction
    public CharacterStats MoverStats { get; set; }

    private int moveCost;
    protected List<Hex> movePath;
    protected Hex InterruptedAtHex;


    [Server]
    public virtual void ServerUse(INetworkedLogger logger)
    {
        //move one hex at a time to ensure we die on correct tile
        //still need to call rpcs after loop to avoid race conditions
        Hex previousHex = this.ActorHex;
        Hex diedOnHex = null;

        foreach (Hex nextHex in this.movePath)
        {
            ActorCharacter.UsedMoves(nextHex.MoveCost());
            this.ActorCharacter.RpcPlaceCharSprite(nextHex.transform.position, true);
            this.MoveToTile(previousHex, nextHex, logger);

            if (this.ActorCharacter.IsDead)
            {
                diedOnHex = nextHex;
                break;
            }

            previousHex = nextHex;
        }

        if (diedOnHex != null)
        {
            //in case sub classes need to do stuff on pathed hexes
            this.InterruptedAtHex = diedOnHex;
        }
    }

    public virtual bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            !this.TargetHex.HoldsACharacter() &&
            !this.TargetHex.HoldsACorpse() &&
            !this.TargetHex.HoldsAnObstacle() &&
            this.ActorCharacter.HasAvailableMoves() &&
            this.ActorCharacter.CanMove &&
            ITargetedAction.ValidateTarget(this))
        {
            //Validate path
            if (this.movePath == null)
            {
                Debug.Log("Client requested move without path to destination");
                return false;
            }

            if (this.moveCost > this.ActorCharacter.RemainingMoves)
            {
                Debug.Log("Client requested move outside his current range");
                return false;
            }

            return true;
        }            
        else
            return false;
    }

    protected void MoveToTile(Hex previousHex, Hex nextHex, INetworkedLogger logger)
    {
        Map.Singleton.MoveCharacter(this.ActorCharacter.CharClassID, previousHex, nextHex);

        if (nextHex.holdsTreasure)
        {
            string message = string.Format("{0} has collected treasure.", this.ActorCharacter.charClass.name);
            logger.RpcLogMessage(message);
            Object.Destroy(Map.Singleton.Treasure);
            GameController.Singleton.SetTreasureOpenedByPlayerID(this.RequestingPlayerID);
        }

        int moveDamage = nextHex.DealsDamageWhenMovedInto();
        if (moveDamage > 0)
        {
            string message;
            if (nextHex.DealsDamageTypeWhenMovedInto() == DamageType.healing)
            {
                this.ActorCharacter.TakeDamage(new Hit(moveDamage, nextHex.DealsDamageTypeWhenMovedInto(), HitSource.Apple));

                message = string.Format("{0} gains {1} life from {2}",
                    this.ActorCharacter.charClass.name,
                    moveDamage,
                    nextHex.holdsHazard);
            }
            else
            {
                this.ActorCharacter.TakeDamage(new Hit(moveDamage, nextHex.DealsDamageTypeWhenMovedInto(), HitSource.FireHazard));
                message = string.Format("{0} takes {1} {2} damage for walking on {3} hazard",
                this.ActorCharacter.charClass.name,
                moveDamage,
                nextHex.DealsDamageTypeWhenMovedInto(),
                nextHex.holdsHazard);
            }
            MasterLogger.Singleton.RpcLogMessage(message);
        }

        if (nextHex.HoldsAHazard() && HazardDataSO.Singleton.IsHazardTypeRemovedWhenWalkedUpon(nextHex.holdsHazard))
            Map.Singleton.hazardManager.DestroyHazardAtPosition(Map.Singleton.hexGrid, nextHex.coordinates.OffsetCoordinatesAsVector());

    }

    private int PreviewMoveToTileDamage(Hex previousHex, Hex nextHex)
    {
        int tileDamage = nextHex.DealsDamageWhenMovedInto();
        int resultingDamage = 0;
        if (tileDamage > 0)
        {
            HitSource hitsource;
            if (nextHex.DealsDamageTypeWhenMovedInto() == DamageType.healing)
                hitsource = HitSource.Apple;
            else
                hitsource = HitSource.FireHazard;
            
             resultingDamage = this.ActorCharacter.CalculateDamageFromHit(new Hit(tileDamage, nextHex.DealsDamageTypeWhenMovedInto(), hitsource));
        }
        return resultingDamage;
    }

    public virtual void SetupPath()
    {
        this.movePath = MapPathfinder.FindMovementPath(this.ActorHex, this.TargetHex, Map.Singleton.hexGrid);
        this.moveCost = MapPathfinder.PathCost(this.movePath);
    }

    public ActionEffectPreview PreviewEffect()
    {
        //move one hex at a time to ensure we die on correct tile
        Hex previousHex = this.ActorHex;
        Hex diedOnHex = null;
        int totalMovementDamage = 0;
        foreach (Hex nextHex in this.movePath)
        {
            totalMovementDamage += this.PreviewMoveToTileDamage(previousHex, nextHex);

            if (totalMovementDamage >= this.ActorCharacter.CurrentLife)
            {
                diedOnHex = nextHex;
                break;
            }

            previousHex = nextHex;
        }

        EffectOnCharacter effectOnCharacter = new EffectOnCharacter(this.ActorCharacter.CharClassID, diedOnHex != null ? diedOnHex.coordinates : this.TargetHex.coordinates, totalMovementDamage, totalMovementDamage);
        ActionEffectPreview effectPreview = new(new EffectOnCharacter[] { effectOnCharacter });
        return effectPreview;
    }
}
