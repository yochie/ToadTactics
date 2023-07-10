using Mirror;

public interface IAction
{
    public int RequestingPlayerID { get; set; }

    public PlayerCharacter ActorCharacter { get; set; }

    public Hex ActorHex { get; set; }

    public NetworkConnectionToClient Sender { get; set; }

    public abstract void ServerUse();

    public abstract bool ServerValidate();
}