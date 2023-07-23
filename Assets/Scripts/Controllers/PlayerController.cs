using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    //TODO : Make into syncvar..
    //0 for host
    //1 for client
    [SyncVar]
    public int playerID;

    [SerializeField]
    private IntGameEventSO onCharacterPlaced;

    [SerializeField]
    private IntIntGameEventSO onCharacterDrafted;

    [SerializeField]
    private IntGameEventSO onCharacterCrowned;

    [SyncVar]
    public int kingClassID;

    #region Startup

    //needs to be in start : https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!this.isOwned)
        {
            GameController.Singleton.NonLocalPlayer = this;
        } else
        {
            GameController.Singleton.LocalPlayer = this;
        }

        GameController.Singleton.playerControllers.Add(this);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        this.kingClassID = -1;

        this.playerID = this.isOwned ? 0 : 1;

        GameController.Singleton.SetScore(this.playerID, 0);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    #endregion

    #region Commands

    [Server]
    public void FakeDraft()
    {
        List<int> usedClasses = new();

        //make characters used by other clients unavailable
        foreach (int classID in GameController.Singleton.draftedCharacterOwners.Keys)
        {
            usedClasses.Add(classID);
        }
        for (int i = 0; i < GameController.Singleton.defaultNumCharsPerPlayer; i++)
        {
            int classID;
            List<int> classIDs = new();
            ClassDataSO.Singleton.GetClassIDs().CopyTo<int>(classIDs);
            do
            {
                classID = classIDs[Random.Range(0, classIDs.Count)];
            }
            while (usedClasses.Contains(classID));
            usedClasses.Add(classID);

            this.CmdDraftCharacter(classID);
        }
    }

    [Command]
    internal void CmdCrownCharacter(int classID, NetworkConnectionToClient sender = null)
    {
        //update GameController (remember: dont update state asynchronously in events to avoir sync bugs)
        this.kingClassID = classID;

        //update GameController (remember: dont update state asynchronously in events to avoir sync bugs)
        GameController.Singleton.CmdCrownCharacter(this.playerID, classID);

        this.TargetRpcOnCharacterCrowned(sender, classID);        
    }

    [Command]
    public void CmdDraftCharacter(int classID)
    {
        //update GameController (remember: dont update state asynchronously in events to avoir sync bugs)
        GameController.Singleton.CmdDraftCharacter(this.playerID, classID);

        //throw event that updates UI elements
        this.RpcOnCharacterDrafted(this.playerID, classID);
    }

    [Command(requiresAuthority = false)]
    public void CmdCreateCharOnBoard(int characterClassID, Hex destinationHex, NetworkConnectionToClient sender = null)
    {
        int ownerPlayerIndex = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        //validate destination
        if (destinationHex == null ||
            !destinationHex.isStartingZone ||
            destinationHex.startZoneForPlayerIndex != ownerPlayerIndex ||
            destinationHex.holdsCharacterWithClassID != -1)
        {
            Debug.Log("Invalid character destination");
            return;
        }

        PlayerCharacter characterPrefab = ClassDataSO.Singleton.GetPrefabByClassID(characterClassID);
        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newCharObject =
            Instantiate(characterPrefab.gameObject, destinationWorldPos, Quaternion.identity);
        PlayerCharacter newChar = newCharObject.GetComponent<PlayerCharacter>();

        newChar.SetOwner(ownerPlayerIndex);
        if(newChar.charClassID == this.kingClassID)
            newChar.isKing = true;
        newChar.transform.position = destinationWorldPos;

        //will actually init character on server using class data and state set above
        NetworkServer.Spawn(newCharObject, connectionToClient);

        //add player to both lists
        GameController.Singleton.playerCharactersNetIDs.Add(characterClassID, newCharObject.GetComponent<NetworkIdentity>().netId);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = characterClassID;
        this.TargetRpcOnCharacterPlaced(sender, characterClassID);


        Map.Singleton.characterPositions[characterClassID] = destinationHex.coordinates;

        GameController.Singleton.CmdNextTurn();
    }
    #endregion

    #region Callbacks    

    [TargetRpc]
    private void TargetRpcOnCharacterPlaced(NetworkConnectionToClient sender, int charClassID)
    {
        this.onCharacterPlaced.Raise(charClassID);
    }

    [ClientRpc]
    private void RpcOnCharacterDrafted(int draftedByPlayerID, int charClassID)
    {
        this.onCharacterDrafted.Raise(draftedByPlayerID, charClassID);
    }

    [TargetRpc]
    private void TargetRpcOnCharacterCrowned(NetworkConnectionToClient sender, int charClassID)
    {
        this.onCharacterCrowned.Raise(charClassID);
    }
    #endregion
}
