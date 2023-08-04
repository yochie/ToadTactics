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

    [SyncVar]
    public int kingClassID;
    
    private readonly SyncList<string> draftedEquipmentIDs = new();

    //equipmentID -> classID
    private readonly SyncDictionary<string, int> assignedEquipments = new();

    [SerializeField]
    private IntGameEventSO onCharacterPlaced;

    [SerializeField]
    private IntIntGameEventSO onCharacterDrafted;

    [SerializeField]
    private IntGameEventSO onCharacterCrowned;

    [SerializeField]
    private StringIntGameEventSO onEquipmentDrafted;

    [SerializeField]
    private StringIntIntGameEventSO onEquipmentAssigned;

    [SerializeField]
    private GameEventSO onLocalPlayerAssignedAllEquipments;

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

        this.draftedEquipmentIDs.Callback += OnDraftedEquipementIDsChanged;
        for (int index = 0; index < draftedEquipmentIDs.Count; index++)
            OnDraftedEquipementIDsChanged(SyncList<string>.Operation.OP_ADD, index, "", draftedEquipmentIDs[index]);
    }

    void OnDraftedEquipementIDsChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        switch (op)
        {
            case SyncList<string>.Operation.OP_ADD:
                //Debug.Log("New equipment added to drafted list in player controller.");
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncList<string>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<string>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<string>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<string>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
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
        foreach (int classID in GameController.Singleton.DraftedCharacterOwners.Keys)
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

    [Command]
    public void CmdAssignEquipment(string equipmentID, int classID, NetworkConnectionToClient sender = null)
    {
        Debug.LogFormat("Assigning equipment {0} to {1} for player {2}", equipmentID, ClassDataSO.Singleton.GetClassByID(classID).name, sender.identity.GetComponent<PlayerController>().playerID);
        this.assignedEquipments.Add(equipmentID, classID);
       
        this.TargetRpcOnEquipmentAssigned(sender, equipmentID, this.playerID, classID);

        string nextToAssign = null;
        foreach(string draftedEquipmentID in this.draftedEquipmentIDs)
        {
            if (!this.assignedEquipments.ContainsKey(draftedEquipmentID))
            {
                nextToAssign = draftedEquipmentID;
                break;
            }
        }

        //tick phase to indicate that this player has finished all assignments
        //if both players have ticked phase while all characters are drafted, it will trigger next phase
        if(nextToAssign == null) 
        {
            Debug.LogFormat("No more equipments to assign, ticking phase.");

            GameController.Singleton.CmdNextTurn();
            this.TargetRpcOnLocalPlayerAssignedAllEquipments(sender);
        } else
        {
            //Debug.LogFormat("Displaying next equipment.");

            GameController.Singleton.equipmentDraftUI.TargetRpcUpdateEquipmentAssignment(target: sender, nextEquipmentID: nextToAssign);
        }        
    }

    [TargetRpc]
    private void TargetRpcOnLocalPlayerAssignedAllEquipments(NetworkConnectionToClient sender)
    {
        this.onLocalPlayerAssignedAllEquipments.Raise();
    }

    [Command]
    public void CmdDraftEquipment(string equipmentID)
    {
        Debug.LogFormat("Player {0} drafted equipment {1}", this.playerID, EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID).name);
        this.draftedEquipmentIDs.Add(equipmentID);
        GameController.Singleton.CmdNextTurn();

        //throw event that updates UI elements
        this.RpcOnEquipmentDrafted(equipmentID, this.playerID);
    }

    [Command(requiresAuthority = false)]
    public void CmdCreateCharOnBoard(int charIDToCreate, Hex destinationHex, NetworkConnectionToClient sender = null)
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

        PlayerCharacter characterPrefab = ClassDataSO.Singleton.GetPrefabByClassID(charIDToCreate);
        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newCharObject =
            Instantiate(characterPrefab.gameObject, destinationWorldPos, Quaternion.identity);
        PlayerCharacter newChar = newCharObject.GetComponent<PlayerCharacter>();

        newChar.SetOwner(ownerPlayerIndex);
        if (newChar.CharClassID == this.kingClassID)
            newChar.SetKing(true);
        newChar.transform.position = destinationWorldPos;

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = charIDToCreate;
        Map.Singleton.characterPositions[charIDToCreate] = destinationHex.coordinates;

        //Apply equipments
        foreach(KeyValuePair<string, int> assignedEquipment in this.assignedEquipments)
        {
            if(assignedEquipment.Value == charIDToCreate)
            {
                newChar.GiveEquipment(assignedEquipment.Key);
            }
        }

        //will actually init character on server using class data and state set above
        NetworkServer.Spawn(newCharObject, connectionToClient);

        GameController.Singleton.PlayerCharactersNetIDs.Add(charIDToCreate, newCharObject.GetComponent<NetworkIdentity>().netId);

        this.TargetRpcOnCharacterPlaced(sender, charIDToCreate);

        GameController.Singleton.CmdNextTurn();
    }
    #endregion

    #region Rpcs    

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

    [ClientRpc]
    private void RpcOnEquipmentDrafted(string equipmentID, int playerID)
    {
        this.onEquipmentDrafted.Raise(equipmentID, playerID);
    }

    [TargetRpc]
    private void TargetRpcOnEquipmentAssigned(NetworkConnectionToClient target, string equipmentID, int playerID, int classID)
    {
        this.onEquipmentAssigned.Raise(equipmentID, playerID, classID);
    }

    #endregion

    #region Utility
    internal bool HasDraftedEquipment(string equipmentID)
    {
        if (this.draftedEquipmentIDs.Contains(equipmentID))
            return true;
        else
            return false;
    }

    internal bool HasAssignedEquipment(string equipmentID)
    {
        if (this.assignedEquipments.ContainsKey(equipmentID))
            return true;
        else
            return false;
    }

    internal bool HasAssignedAllEquipments()
    {
        foreach(string equipmentID in this.draftedEquipmentIDs)
        {
            if (!this.assignedEquipments.ContainsKey(equipmentID))
                return false;
        }
        return true;
    }

    internal List<string> GetDraftedEquipmentIDs()
    {
        List<string> copy = new(this.draftedEquipmentIDs);
        return copy;
    }

    internal string GetUnassignedEquipmentID()
    {
        string nextToAssign = null;
        foreach (string draftedEquipmentID in this.draftedEquipmentIDs)
        {
            if (!this.assignedEquipments.ContainsKey(draftedEquipmentID))
            {
                nextToAssign = draftedEquipmentID;
                break;
            }
        }

        return nextToAssign;

    }
    #endregion
}
