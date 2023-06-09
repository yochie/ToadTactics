using Mirror;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : NetworkBehaviour
{
    GameController gc;
    //0 for host
    //1 for client
    public int playerIndex;

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.gc = GameController.Singleton;

        if (isServer && this.isOwned) {
            this.playerIndex = 0;
        } else if (isServer && !this.isOwned)
        {
            this.playerIndex = 1;
        } else if (!isServer && this.isOwned)
        {
            this.playerIndex = 1;
        } else if (!isServer && !this.isOwned)
        {
            this.playerIndex = 0;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        this.gc.LocalPlayer = this;

        //for now just choose random chars
        //TODO : fill these using draft eventually
        for (int i = 0; i < this.gc.CharacterSlotsUI.Length; i++)
        {
            int prefabIndex = Random.Range(0, this.gc.AllPlayerCharPrefabs.Length - 1);
            PlayerCharacter newChar = this.gc.AllPlayerCharPrefabs[prefabIndex].GetComponent<PlayerCharacter>();
            CharacterSlotUI slot = this.gc.CharacterSlotsUI[i];

            slot.GetComponent<Image>().sprite = newChar.GetComponent<SpriteRenderer>().sprite;

            slot.HoldsPlayerCharacterWithIndex = prefabIndex;
        }
    }

    [Command]
    public void CmdCreateChar(int playerCharacterIndex, Hex destinationHex)
    {
        //Debug.Log(destinationHex.startZoneForPlayerIndex);
        //Debug.Log(this.playerIndex);
        
        //validate destination
        if (destinationHex == null ||
            !destinationHex.isStartingZone ||
            destinationHex.startZoneForPlayerIndex != this.playerIndex ||
            destinationHex.holdsCharacter != null)
        {
            Debug.Log("Invalid character destination");
            return;
        }

        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newChar = 
            Instantiate(gc.AllPlayerCharPrefabs[playerCharacterIndex], destinationWorldPos, Quaternion.identity);
        //TODO : Create other classes and set their name in prefabs
        newChar.GetComponent<PlayerCharacter>().Initialize(this.gc.AllClasses["Barbarian"]);
        NetworkServer.Spawn(newChar, connectionToClient);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacter = newChar.GetComponent<PlayerCharacter>();

        //this.gc.map.PlacePlayerChar();


        this.gc.map.RpcPlaceChar(newChar, destinationWorldPos);
    }

    public override void OnStartServer() 
    {

    }

    public override void OnStopServer() {

    }


}
