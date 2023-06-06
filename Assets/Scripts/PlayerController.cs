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
        Debug.Log(this.gc.PlayerChars);
        Debug.Log(newChar);

        NetworkServer.Spawn(newChar, connectionToClient);

        this.RpcPlaceChar(newChar);
    }

    [ClientRpc]
    public void RpcPlaceChar(GameObject character)
    {
        //Debug.Log("calling rpc on client");
        //Debug.Log(gc.map.GetHex(this.playerIndex, 0));
        //character.transform.SetParent(gc.map.GetHex(this.playerIndex, 0).transform, false);
        Hex destination = gc.map.GetHex(this.playerIndex, 0);
        character.transform.position = destination.transform.position + Map.characterOffsetOnMap;
        //character.transform.localPosition = new Vector3(0, 0, -0.1f);

    }

    public override void OnStartServer() 
    {

    }

    public override void OnStopServer() {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
