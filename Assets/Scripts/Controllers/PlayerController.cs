using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    //0 for host
    //1 for client
    public int playerID;

    [SerializeField]
    private IntGameEventSO onCharacterPlaced;

    [SerializeField]
    private IntIntGameEventSO onCharacterDrafted;

    //maps classID to PlayerCharacter
    public readonly SyncDictionary<int, uint> playerCharactersNetIDs = new();
    public readonly Dictionary<int, PlayerCharacter> playerCharacters = new();

    #region Startup

    //needs to be in start : https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //setup sync dict callbacks to sync actual objects from netids
        this.playerCharactersNetIDs.Callback += OnPlayerCharactersNetIDsChange;
        foreach (KeyValuePair<int, uint> kvp in playerCharactersNetIDs)
            OnPlayerCharactersNetIDsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);

        if (isServer && this.isOwned) {
            this.playerID = 0;
        } else if (isServer && !this.isOwned)
        {
            this.playerID = 1;
        } else if (!isServer && this.isOwned)
        {
            this.playerID = 1;
        } else if (!isServer && !this.isOwned)
        {
            this.playerID = 0;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GameController.Singleton.LocalPlayer = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameController.Singleton.playerControllers.Add(this);
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
        //for now just choose random chars
        //TODO : fill these using draft eventually
        List<int> usedClasses = new();

        //make characters used by other clients unavailable
        foreach (int classID in GameController.Singleton.characterOwners.Keys)
        {
            usedClasses.Add(classID);
        }
        //Debug.LogFormat("usedClasses counts {0}", usedClasses.Count);
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
    public void CmdDraftCharacter(int classID)
    {
        //update GameController (remember: dont update state asynchronously in events to avoir sync bugs)
        GameController.Singleton.CmdAddCharToCharacterOwners(this.playerID, classID);

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

        GameObject characterPrefab = ClassDataSO.Singleton.GetPrefabByClassID(characterClassID);
        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newCharObject =
            Instantiate(characterPrefab, destinationWorldPos, Quaternion.identity);
        PlayerCharacter newChar = newCharObject.GetComponent<PlayerCharacter>();

        newChar.SetOwner(ownerPlayerIndex);
        newChar.transform.position = destinationWorldPos;
        NetworkServer.Spawn(newCharObject, connectionToClient);

        //add player to both lists
        GameController.Singleton.playerCharactersNetIDs.Add(characterClassID, newCharObject.GetComponent<NetworkIdentity>().netId);
        this.playerCharactersNetIDs.Add(characterClassID, newCharObject.GetComponent<NetworkIdentity>().netId);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = characterClassID;
        this.RpcOnCharacterPlaced(sender, characterClassID);


        Map.Singleton.characterPositions[characterClassID] = destinationHex.coordinates;

        GameController.Singleton.NextTurn();
    }
    #endregion

    #region Callbacks
    //callback for syncing list of active characters
    [Client]
    private void OnPlayerCharactersNetIDsChange(SyncIDictionary<int, uint>.Operation op, int key, uint netidArg)
    {
        switch (op)
        {
            case SyncDictionary<int, uint>.Operation.OP_ADD:
                this.playerCharacters[key] = null;
                if (NetworkClient.spawned.TryGetValue(netidArg, out NetworkIdentity netidObject))
                    this.playerCharacters[key] = netidObject.gameObject.GetComponent<PlayerCharacter>();
                else
                    StartCoroutine(PlayerFromNetIDCoroutine(key, netidArg));
                break;
            case SyncDictionary<int, uint>.Operation.OP_SET:
                // entry changed
                break;
            case SyncDictionary<int, uint>.Operation.OP_REMOVE:
                // entry removed
                break;
            case SyncDictionary<int, uint>.Operation.OP_CLEAR:
                // Dictionary was cleared
                break;
        }
    }

    //coroutine for finishing syncvar netid matching
    [Client]
    private IEnumerator PlayerFromNetIDCoroutine(int key, uint netIdArg)
    {
        while (this.playerCharacters[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
                this.playerCharacters[key] = identity.gameObject.GetComponent<PlayerCharacter>();
        }
    }

    [TargetRpc]
    private void RpcOnCharacterPlaced(NetworkConnectionToClient sender, int charClassID)
    {
        this.onCharacterPlaced.Raise(charClassID);
    }

    [ClientRpc]
    private void RpcOnCharacterDrafted(int draftedByPlayerID, int charClassID)
    {
        this.onCharacterDrafted.Raise(draftedByPlayerID, charClassID);
    }
    #endregion
}
