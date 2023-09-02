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
    protected Hex interruptedAtHex;

    [Server]
    public virtual void ServerUse(INetworkedLogger logger)
    {
        //move one hex at a time to ensure we die on correct tile
        //still need to call rpcs after loop to avoid race conditions
        Hex previousHex = this.ActorHex;
        Hex diedOnHex = null;
        foreach(Hex nextHex in this.movePath)
        {
            ActorCharacter.UsedMoves(nextHex.MoveCost());
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
                this.ActorCharacter.TakeDamage(new Hit(moveDamage, nextHex.DealsDamageTypeWhenMovedInto()));

                string message = string.Format("{0} takes {1} {2} damage for walking on {3} hazard",
                    this.ActorCharacter.charClass.name,
                    moveDamage,
                    nextHex.DealsDamageTypeWhenMovedInto(),
                    nextHex.holdsHazard);
                MasterLogger.Singleton.RpcLogMessage(message);

                if (this.ActorCharacter.IsDead)
                {
                    diedOnHex = nextHex;
                    break;
                }
            }

            previousHex = nextHex;
        }

        if (diedOnHex != null)
        {
            //in case sub classes need to do stuff on pathed hexes
            this.interruptedAtHex = diedOnHex;
            this.ActorCharacter.RpcPlaceCharSprite(diedOnHex.transform.position);
        }
        else
        {
            this.ActorCharacter.RpcPlaceCharSprite(this.TargetHex.transform.position);
            MapInputHandler.Singleton.TargetRpcSelectHex(this.RequestingClient, this.TargetHex);
        }      
    }

    [Server]
    public bool ServerValidate()
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
            //this.movePath = MapPathfinder.FindMovementPath(this.ActorHex, this.TargetHex, Map.Singleton.hexGrid);
            if (this.movePath == null)
            {
                Debug.Log("Client requested move without path to destination");
                return false;
            }

            //this.moveCost = MapPathfinder.PathCost(path);
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

    [Server]
    public void SetupPath()
    {
        this.movePath = MapPathfinder.FindMovementPath(this.ActorHex, this.TargetHex, Map.Singleton.hexGrid);
        this.moveCost = MapPathfinder.PathCost(this.movePath);
    }
}
