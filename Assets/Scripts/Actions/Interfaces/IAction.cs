using Mirror;
using UnityEngine;

public interface IAction
{
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    public abstract void ServerUse();

    public abstract bool ServerValidate();

    public static bool ValidateBasicAction(IAction action)
    {
        if (action.ActorCharacter != null &&
            action.ActorHex != null &&
            action.RequestingPlayerID != -1 &&
            action.ActorHex.HoldsACharacter() &&
            action.ActorHex.GetHeldCharacterObject() == action.ActorCharacter &&
            action.RequestingPlayerID == action.ActorCharacter.OwnerID &&
            GameController.Singleton.ItsThisPlayersTurn(action.RequestingPlayerID) &&
            GameController.Singleton.ItsThisCharactersTurn(action.ActorCharacter.CharClassID))
            return true;
        else
        {
            Debug.Log("Basic action validation failed");
            return false;
        }
    }
}