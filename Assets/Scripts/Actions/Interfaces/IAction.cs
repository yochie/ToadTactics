using Mirror;
using UnityEngine;

public interface IAction
{
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient RequestingClient { get; set; }

    public abstract void ServerUse(INetworkedLogger logger);

    public abstract bool ServerValidate();

    public static bool ValidateBasicAction(IAction action)
    {
        bool isValid = true;
        if (!(action is IOutOfControlAction) &&
            !(action.RequestingPlayerID == action.ActorCharacter.OwnerID &&
              GameController.Singleton.ItsThisPlayersTurn(action.RequestingPlayerID) &&
              GameController.Singleton.ItsThisCharactersTurn(action.ActorCharacter.CharClassID)))
            isValid = false;
        
        if (!(action.ActorCharacter != null &&
            action.ActorHex != null &&
            action.RequestingPlayerID != -1 &&
            action.ActorHex.HoldsACharacter() &&
            action.ActorHex.GetHeldCharacterObject() == action.ActorCharacter))            
            isValid = false;

        if (!isValid) {
            Debug.Log("Basic action validation failed");
        }

        return isValid;
    }
}