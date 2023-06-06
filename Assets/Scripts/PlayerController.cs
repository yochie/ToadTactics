using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerController : NetworkBehaviour
{
    public static int numPlayers = 0;

    GameController gc;
    public int playerIndex;

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.playerIndex = numPlayers;
        PlayerController.numPlayers++;

        this.gc = GameObject.Find("GameController").GetComponent<GameController>();
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
