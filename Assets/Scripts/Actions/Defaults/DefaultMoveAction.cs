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

    //IMoveAction
    public CharacterStats MoverStats { get; set; }

    private int moveCost;

    [Server]
    public void ServerUse()
    {

        //actually moves character
        this.ActorCharacter.RpcPlaceChar(this.TargetHex.transform.position);

        //update state
        ActorCharacter.UseMoves(this.moveCost);
        this.TargetHex.holdsCharacterWithClassID = this.ActorHex.holdsCharacterWithClassID;
        Map.Singleton.characterPositions[this.ActorCharacter.charClassID] = this.TargetHex.coordinates;
        MapInputHandler.Singleton.TargetRpcUpdateSelectedHex(this.RequestingClient, this.TargetHex);
        this.ActorHex.ClearCharacter();

        //Update UI/Gamecontroller
        if (this.ActorCharacter.CanMoveDistance() == 0)
        {
            MainHUD.Singleton.RpcGrayOutMoveButton(this.RequestingClient);
            if (!ActorCharacter.hasAttacked)
                MapInputHandler.Singleton.TargetRpcSetControlModeOnClient(this.RequestingClient, ControlMode.attack);
        }
    }

    [Server]
    public bool ServerValidate()
    {        
        if (this.ActorCharacter != null &&
            this.ActorHex != null &&
            this.TargetHex != null &&
            !this.TargetHex.HoldsACharacter() &&
            this.TargetHex.holdsObstacle == ObstacleType.none &&
            this.RequestingPlayerID != -1 &&
            this.ActorHex.HoldsACharacter() &&
            this.ActorHex.GetHeldCharacterObject() == this.ActorCharacter &&                        
            this.ActorCharacter.CanMoveDistance() > 0 &&
            this.RequestingPlayerID == this.ActorCharacter.ownerID &&
            GameController.Singleton.ItsThisPlayersTurn(this.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(this.ActorCharacter.charClassID))
        {
            //Validate path
            List<Hex> path = MapPathfinder.FindMovementPath(this.ActorHex, this.TargetHex, Map.Singleton.hexGrid);
            if (path == null)
            {
                return false;
            }

            this.moveCost = MapPathfinder.PathCost(path);
            if (this.moveCost > this.ActorCharacter.CanMoveDistance())
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
