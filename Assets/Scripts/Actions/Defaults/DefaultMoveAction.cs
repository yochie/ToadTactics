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
    private List<Hex> movePath;

    [Server]
    public void ServerUse()
    {

        //actually moves character
        this.ActorCharacter.RpcPlaceChar(this.TargetHex.transform.position);

        //update state

        foreach(Hex hex in this.movePath)
        {
            if (hex.DealsDamageWhenMovedInto() > 0)
                this.ActorCharacter.TakeDamage(hex.DealsDamageWhenMovedInto(), hex.DealsDamageTypeWhenMovedInto());
            if (hex.holdsTreasure)
            {
                Debug.Log("{0} has moved on treasure. He will be assigned extra treasure.");
                Object.Destroy(Map.Singleton.Treasure);
                GameController.Singleton.SetTreasureOpenedByPlayerID(this.RequestingPlayerID);
            }
        }

        ActorCharacter.UsedMoves(this.moveCost);
        this.TargetHex.holdsCharacterWithClassID = this.ActorHex.holdsCharacterWithClassID;
        Map.Singleton.characterPositions[this.ActorCharacter.CharClassID] = this.TargetHex.coordinates;
        MapInputHandler.Singleton.TargetRpcSelectHex(this.RequestingClient, this.TargetHex);
        this.ActorHex.ClearCharacter();
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
