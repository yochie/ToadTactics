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

    private readonly SyncList<string> equipmentIDsToAssign = new();

    //equipmentID -> classID
    private readonly SyncDictionary<string, int> assignedEquipments = new();
    public Dictionary<string, int> AssignedEquipmentsCopy  {
        get {
            Dictionary<string, int> copyToReturn = new(this.assignedEquipments);
            return copyToReturn;
        }            
    }

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

        this.equipmentIDsToAssign.Callback += OnDraftedEquipementIDsChanged;
        for (int index = 0; index < equipmentIDsToAssign.Count; index++)
            OnDraftedEquipementIDsChanged(SyncList<string>.Operation.OP_ADD, index, "", equipmentIDsToAssign[index]);
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
        if (GameController.Singleton.CurrentPhaseID != GamePhaseID.characterDraft)
        {
            Debug.Log("Attempting to draft while in wrong phase. Ignoring.");
            return;
        }

        if (this.kingClassID != -1)
        {
            Debug.Log("Attempting to crown character but you already have king. Ignoring.");
            return;
        }

        Debug.LogFormat("Player {0} crowned character {1}", this.playerID, ClassDataSO.Singleton.GetClassByID(classID).name);

        this.TargetRpcOnCharacterCrowned(sender, classID);

        this.kingClassID = classID;

        //notify GameController so that he changes scene once done
        GameController.Singleton.CharacterCrowned();        
    }

    [Command]
    public void CmdDraftCharacter(int classID)
    {
        if (GameController.Singleton.CurrentPhaseID != GamePhaseID.characterDraft)
        {
            Debug.Log("Attempting to draft while in wrong phase. Ignoring.");
            return;
        }

        if (GameController.Singleton.PlayerTurn != this.playerID)
        {
            Debug.Log("Attempting to draft while it isn't your turn. Ignoring.");
            return;
        }

        if (GameController.Singleton.CharacterHasBeenDrafted(classID))
        {
            Debug.Log("Attempting to draft character that has already been drafted. Ignoring.");
            return;
        }

        Debug.LogFormat("Player {0} drafted character {1}", this.playerID, ClassDataSO.Singleton.GetClassByID(classID).name);

        //throw event that updates UI elements
        this.RpcOnCharacterDrafted(this.playerID, classID);

        GameController.Singleton.DraftCharacter(this.playerID, classID);

        GameController.Singleton.NextTurn();
    }

    [Command]
    public void CmdAssignEquipment(string equipmentID, int classID, NetworkConnectionToClient sender = null)
    {
        if (GameController.Singleton.CurrentPhaseID != GamePhaseID.equipmentDraft)
        {
            Debug.Log("Attempting to assign equipment while in wrong phase. Ignoring.");
            return;
        }

        PlayerController otherPlayer = GameController.Singleton.playerControllers[GameController.Singleton.OtherPlayer(this.playerID)];
        if (this.HasAssignedEquipment(equipmentID) || otherPlayer.HasAssignedEquipment(equipmentID))
        {
            Debug.Log("Attempting to assign equipment that has already been assigned. Ignoring.");
            return;
        }

        Debug.LogFormat("Player {2} assigned equipment {0} to {1}", equipmentID, ClassDataSO.Singleton.GetClassByID(classID).name, sender.identity.GetComponent<PlayerController>().playerID);
        this.assignedEquipments.Add(equipmentID, classID);
        PlayerCharacter charToUpdate = GameController.Singleton.PlayerCharactersByID[classID];
        charToUpdate.ApplyEquipment(equipmentID);
        GameController.Singleton.equipmentDraftUI.TargetRPCUpdateCharacterStats(sender, classID, charToUpdate.CurrentStats, charToUpdate.IsKing);

        //Updates UI stuff hooked onto this event (display icon in character panel)
        this.TargetRpcOnEquipmentAssigned(sender, equipmentID, this.playerID, classID);

        string nextToAssign = this.GetUnassignedEquipmentID();

        //tick phase to indicate that this player has finished all assignments
        //if both players have ticked phase while all characters are drafted, it will trigger next phase
        if (nextToAssign == null) 
        {
            Debug.LogFormat("No more equipments to assign, ticking phase.");
            this.TargetRpcOnLocalPlayerAssignedAllEquipments(sender);
            GameController.Singleton.NextTurn();
        } else
        {
            //Debug.LogFormat("Displaying next equipment.");
            GameController.Singleton.equipmentDraftUI.TargetRpcUpdateEquipmentAssignment(target: sender, nextEquipmentID: nextToAssign);
        }        
    }

    [Command]
    public void CmdDraftEquipment(string equipmentID)
    {
        if (GameController.Singleton.CurrentPhaseID != GamePhaseID.equipmentDraft)
        {
            Debug.Log("Attempting to draft equipment while in wrong phase. Ignoring.");
            return;
        }

        if (GameController.Singleton.PlayerTurn != this.playerID)
        {
            Debug.Log("Attempting to draft equipment while it isn't your turn. Ignoring.");
            return;
        }

        PlayerController otherPlayer = GameController.Singleton.playerControllers[GameController.Singleton.OtherPlayer(this.playerID)];
        if (this.HasDraftedEquipment(equipmentID) || otherPlayer.HasDraftedEquipment(equipmentID))
        {
            Debug.Log("Attempting to draft equipment that has already been drafted. Ignoring.");
            return;
        }

        Debug.LogFormat("Player {0} drafted equipment {1}", this.playerID, EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID).name);

        this.AddEquipmentIDToAssign(equipmentID);
        //throw event that updates UI elements
        this.RpcOnEquipmentDrafted(equipmentID, this.playerID);
        GameController.Singleton.NextTurn();
    }

    [TargetRpc]
    private void TargetRpcOnLocalPlayerAssignedAllEquipments(NetworkConnectionToClient sender)
    {
        this.onLocalPlayerAssignedAllEquipments.Raise();
    }

    [Command(requiresAuthority = false)]
    public void CmdPlaceCharOnBoard(int charIDToPlace, Hex destinationHex)
    {
        if (GameController.Singleton.PlayerTurn != this.playerID)
        {
            Debug.LogFormat("Player {0} attempted to place character on board while it wasn't his turn. Ignoring.", this.playerID);
            return;
        }

        if (!GameController.Singleton.DraftedCharacterOwners.ContainsKey(charIDToPlace))
        {
            Debug.LogFormat("Player {0} attempted to place character on board but couldn't find owner from drafted character owners.", this.playerID);
            return;
        }
        int ownerPlayerIndex = GameController.Singleton.DraftedCharacterOwners[charIDToPlace];

        if (!GameController.Singleton.PlayerCharactersByID.ContainsKey(charIDToPlace))
        {
            Debug.LogFormat("Player {0} attempted to place character {1} on board but couldn't find character by ID in gamecontroller.", this.playerID, charIDToPlace);
            return;
        }
        PlayerCharacter toPlace = GameController.Singleton.PlayerCharactersByID[charIDToPlace];

        //validate destination
        if (destinationHex == null ||
            !destinationHex.isStartingZone ||
            destinationHex.startZoneForPlayerIndex != ownerPlayerIndex ||
            destinationHex.HoldsACharacter())
        {
            Debug.LogFormat("Player {0} attempted to place character on board but destination wasn't valid.", this.playerID);
            return;
        }

        Vector3 destinationWorldPos = destinationHex.transform.position;

        toPlace.RpcPlaceAndSetVisible(true, destinationWorldPos);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = charIDToPlace;
        Map.Singleton.characterPositions[charIDToPlace] = destinationHex.coordinates;

        this.TargetRpcOnCharacterPlaced(target: this.connectionToClient, charIDToPlace);
        GameController.Singleton.NextTurn();
    }

    //withMap allows setting a more approriate parent in object hierarchy
    //when false, will simply create a top level of hierarchy
    [Server]
    public void CreateCharacter(int charIDToCreate, bool withMap = true)
    { 
        int ownerPlayerIndex = GameController.Singleton.DraftedCharacterOwners[charIDToCreate];
        PlayerCharacter characterPrefab = ClassDataSO.Singleton.GetPrefabByClassID(charIDToCreate);
        GameObject newCharObject;
        if (withMap)
            newCharObject = Instantiate(characterPrefab.gameObject, Vector3.zero, Quaternion.identity, Map.Singleton.MapObjectParentTransform);
        else
            newCharObject = Instantiate(characterPrefab.gameObject, Vector3.zero, Quaternion.identity);

        PlayerCharacter newChar = newCharObject.GetComponent<PlayerCharacter>();
        newChar.SetOwner(ownerPlayerIndex);
        if (newChar.CharClassID == this.kingClassID)
            newChar.SetKing(true);

        //Adds to owned equipments list, applied upon Init
        foreach (KeyValuePair<string, int> assignedEquipment in this.assignedEquipments)
        {
            if (assignedEquipment.Value == charIDToCreate)
            {
                newChar.GiveEquipment(assignedEquipment.Key);
            }
        }

        //will actually init character on server using class data and state set above
        NetworkServer.Spawn(newCharObject, connectionToClient);

        GameController.Singleton.PlayerCharactersNetIDs.Add(charIDToCreate, newCharObject.GetComponent<NetworkIdentity>().netId);

    }

    [Server]
    public void AddEquipmentIDToAssign(string equipmentID)
    {
        this.equipmentIDsToAssign.Add(equipmentID);
    }
    #endregion

    #region Rpcs    

    [TargetRpc]
    private void TargetRpcOnCharacterPlaced(NetworkConnectionToClient target, int charClassID)
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

    [ClientRpc]
    internal void RpcClearStartZones()
    {
        foreach (Hex hex in Map.Singleton.hexGrid.Values)
        {
            if(hex.isStartingZone)
                hex.drawer.ClearStartZone();
        }
        GameController.Singleton.CmdNotifyStartZonesCleared(GameController.Singleton.LocalPlayer.playerID);
    }
    #endregion

    #region Utility
    internal bool HasDraftedEquipment(string equipmentID)
    {
        if (this.equipmentIDsToAssign.Contains(equipmentID))
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
        foreach(string equipmentID in this.equipmentIDsToAssign)
        {
            if (!this.assignedEquipments.ContainsKey(equipmentID))
                return false;
        }
        return true;
    }

    internal List<string> GetDraftedEquipmentIDsClone()
    {
        List<string> copy = new(this.equipmentIDsToAssign);
        return copy;
    }

    internal string GetUnassignedEquipmentID()
    {
        string nextToAssign = null;
        foreach (string equipmentToAssign in this.equipmentIDsToAssign)
        {
            if (!this.assignedEquipments.ContainsKey(equipmentToAssign))
            {
                nextToAssign = equipmentToAssign;
                break;
            }
        }
        return nextToAssign;
    }
    #endregion
}
