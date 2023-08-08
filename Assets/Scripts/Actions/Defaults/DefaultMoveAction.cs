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

    [Server]
    public void ServerUse()
    {

        //actually moves character
        this.ActorCharacter.RpcPlaceChar(this.TargetHex.transform.position);

        //update state
        ActorCharacter.UsedMoves(this.moveCost);
        this.TargetHex.holdsCharacterWithClassID = this.ActorHex.holdsCharacterWithClassID;
        Map.Singleton.characterPositions[this.ActorCharacter.CharClassID] = this.TargetHex.coordinates;
        MapInputHandler.Singleton.TargetRpcSelectHex(this.RequestingClient, this.TargetHex);
        this.ActorHex.ClearCharacter();

        //Update UI/Gamecontroller
        if (this.ActorCharacter.RemainingMoves == 0)
        {
            MainHUD.Singleton.TargetRpcGrayOutMoveButton(this.RequestingClient);
            if (this.ActorCharacter.HasAvailableAttacks())
                MapInputHandler.Singleton.TargetRpcSetControlMode(this.RequestingClient, ControlMode.attack);
        }
    }

    [Server]
    public bool ServerValidate()
    {        
        if (this.ActorCharacter != null &&
            this.ActorHex != null &&
            this.TargetHex != null &&
            !this.TargetHex.HoldsACharacter() &&
            !this.TargetHex.HoldsACorpse() &&
            this.TargetHex.holdsObstacle == ObstacleType.none &&
            this.RequestingPlayerID != -1 &&
            this.ActorHex.HoldsACharacter() &&
            this.ActorHex.GetHeldCharacterObject() == this.ActorCharacter &&                        
            this.ActorCharacter.RemainingMoves > 0 &&
            this.ActorCharacter.CanMove &&
            this.RequestingPlayerID == this.ActorCharacter.OwnerID &&
            GameController.Singleton.ItsThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(this.ActorCharacter.CharClassID) &&
            ITargetedAction.ValidateTarget(this))
        {
            //Validate path
            List<Hex> path = MapPathfinder.FindMovementPath(this.ActorHex, this.TargetHex, Map.Singleton.hexGrid);
            if (path == null)
            {
                return false;
            }

            this.moveCost = MapPathfinder.PathCost(path);
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
}
