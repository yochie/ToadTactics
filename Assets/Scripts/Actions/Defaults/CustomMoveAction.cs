using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class CustomMoveAction : DefaultMoveAction, IOutOfControlAction
{
    //same as default but skip usedMove() call on character
    [Server]
    public override void ServerUse(INetworkedLogger logger)
    {
        //move one hex at a time to ensure we die on correct tile
        //still need to call rpcs after loop to avoid race conditions
        Hex previousHex = this.ActorHex;
        Hex diedOnHex = null;
        foreach (Hex nextHex in this.movePath)
        {
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
            this.ActorCharacter.RpcPlaceCharSprite(diedOnHex.transform.position);
        }
        else
        {
            this.ActorCharacter.RpcPlaceCharSprite(this.TargetHex.transform.position);
        }
    }

    //Same as default but skip on checking if character has available moves, can move and movecost
    [Server]
    public override bool ServerValidate()
    {
        if (IAction.ValidateBasicAction(this) &&
            !this.TargetHex.HoldsACharacter() &&
            !this.TargetHex.HoldsACorpse() &&
            !this.TargetHex.HoldsAnObstacle() &&
            ITargetedAction.ValidateTarget(this))
        {
            //Validate path
            if (this.movePath == null)
            {
                Debug.Log("Client requested move without path to destination");
                return false;
            }

            return true;
        }
        else
            return false;
    }
}
