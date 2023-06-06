using Mirror;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{
    public static int numPlayers = 0;

    GameController gc;
    public int playerIndex;

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.gc = GameController.Singleton;

        //TODO remake player index by setting on server and syncing to clients
        this.playerIndex = numPlayers;
        PlayerController.numPlayers++;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        PlayerController.numPlayers--;

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Debug.Log("new locally owned player");
        this.CmdCreateChar();

        //for now just choose random chars
        //TODO : fill these using draft eventually
        for (int i = 0; i < this.gc.CharacterSlotsUI.Length; i++)
        {
            int prefabIndex = Random.Range(0, this.gc.AllPlayerCharPrefabs.Length - 1);
            PlayerCharacter newChar = this.gc.AllPlayerCharPrefabs[prefabIndex].GetComponent<PlayerCharacter>();
            this.gc.CharacterSlotsUI[i].sprite = newChar.GetComponent<SpriteRenderer>().sprite;
        }
    }

    [Command]
    public void CmdCreateChar()
    {
        GameObject newChar = 
            Instantiate(gc.AllPlayerCharPrefabs[this.playerIndex], new Vector3(0, 0, -0.1f), Quaternion.identity);

        NetworkServer.Spawn(newChar, connectionToClient);

        this.RpcPlaceChar(newChar);
    }

    [ClientRpc]
    public void RpcPlaceChar(GameObject character)
    {
        Hex destination = gc.map.GetHex(this.playerIndex, 0);
        character.transform.position = destination.transform.position + Map.characterOffsetOnMap;
    }

    public override void OnStartServer() 
    {

    }

    public override void OnStopServer() {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
