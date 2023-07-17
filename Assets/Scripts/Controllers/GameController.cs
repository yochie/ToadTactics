using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

//RTODO: review game progression mechanic
public class GameController : NetworkBehaviour
{
    #region Editor vars

    [SerializeField]
    private MapInputHandler inputHandler;

    public uint defaultNumCharsPerPlayer = 3;

    #endregion

    #region Event vars
    [SerializeField]
    private GameEventSO onCharAddedToTurnOrder;

    [SerializeField]
    private IntGameEventSO onTurnOrderIndexChanged;

    [SerializeField]
    private GameEventSO onLocalPlayerTurnStart;

    [SerializeField]
    private GameEventSO onLocalPlayerTurnEnd;

    [SerializeField]
    private GameEventSO onInitGameplayMode;

    #endregion

    #region Runtime vars

    public static GameController Singleton { get; private set; }

    private bool waitingForClientSpawns;

    public PlayerController LocalPlayer { get; set; }

    //Only filled on server
    public List<PlayerController> playerControllers = new();

    #endregion

    #region Synced vars

    //maps classID to playerID
    public readonly SyncDictionary<int, int> characterOwners = new();

    //maps classID to PlayerCharacter
    public readonly SyncDictionary<int, uint> playerCharactersNetIDs = new();
    public readonly Dictionary<int, PlayerCharacter> playerCharacters = new();

    //maps character initiative to classID
    public readonly SyncIDictionary<float, int> sortedTurnOrder = new SyncIDictionary<float, int>(new SortedList<float, int>());

    //index of currently playing character during gameplay phase
    [SyncVar(hook = nameof(OnTurnOrderIndexChanged))]
    public int turnOrderIndex;

    //currently playing playerID
    [SyncVar(hook = nameof(OnPlayerTurnChanged))]
    public int playerTurn;

    [SyncVar(hook = nameof(OnGamePhaseChanged))]
    public GamePhase currentPhase;
    #endregion

    #region Startup

    private void Awake()
    {
        GameController.Singleton = this;
        this.waitingForClientSpawns = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //setup sync dict callbacks to sync actual objects from netids
        this.playerCharactersNetIDs.Callback += OnPlayerCharactersNetIDsChange;
        foreach (KeyValuePair<int, uint> kvp in playerCharactersNetIDs)
            OnPlayerCharactersNetIDsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        this.SetPhase(GamePhase.waitingForClient);
        this.turnOrderIndex = -1;
        this.playerTurn = -1;

        Map.Singleton.Initialize();

    }

    #endregion

    #region Callbacks
    //TODO : delete or move to mainhud
    //callback for gamemode UI
    [Client]
    private void OnGamePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        MainHUD.Singleton.phaseLabel.text = newPhase.ToString();
    }

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

    #endregion

    #region Events
    [ClientRpc]
    private void RpcOnCharAddedToTurnOrder()
    {
        this.onCharAddedToTurnOrder.Raise();
    }

    //called on on all clients by syncvar hook
    [Client]
    private void OnTurnOrderIndexChanged(int prevTurnIndex, int newTurnIndex)
    {
        this.onTurnOrderIndexChanged.Raise(newTurnIndex);

    }

    [ClientRpc]
    private void RpcOnInitGameplayMode()
    {
        this.onInitGameplayMode.Raise();
    }

    //called on on all clients by syncvar hook
    [Client]
    private void OnPlayerTurnChanged(int _, int newPlayerID)
    {
        if (newPlayerID == -1)
            //we havent started game yet
            return;

        if (newPlayerID == this.LocalPlayer.playerID)
        {
            this.onLocalPlayerTurnStart.Raise();
        }
        else
        {
            this.onLocalPlayerTurnEnd.Raise();
        }
    }
    #endregion

    #region Rpcs



    #endregion

    #region Commands

    //ACTUAL GAME START once everything is ready on client
    [Command(requiresAuthority = false)]
    private void CmdStartPlaying()
    {
        foreach(PlayerController player in this.playerControllers)
        {
            player.FakeDraft();
        }
        this.SetPhase(GamePhase.characterPlacement);
        this.playerTurn = 0;
        //this.RpcActivateEndTurnButton();
    }

    [Command(requiresAuthority = false)]
    public void CmdDraftCharacter(int draftedByPlayerID, int classID)
    {
        float initiative = ClassDataSO.Singleton.GetClassByID(classID).stats.initiative;
        if (Utility.DictContainsValue(this.sortedTurnOrder, classID))
        {
            Debug.Log("Error : Character is already in turnOrder.");
            return;
        }
        this.characterOwners.Add(classID, draftedByPlayerID);
        this.sortedTurnOrder.Add(initiative, classID);
        this.RpcOnCharAddedToTurnOrder();
    }

    [Server]
    private void NextCharacterTurn()
    {
        //loops through turn order        
        this.turnOrderIndex++;
        if (this.turnOrderIndex >= this.sortedTurnOrder.Count)
            this.turnOrderIndex = 0;

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = -1;
        int i = 0;
        foreach (int classID in this.sortedTurnOrder.Values)
        {
            if (i == this.turnOrderIndex)
            {
                currentCharacterClassID = classID;
            }
            i++;
        }
        if (currentCharacterClassID == -1)
        {
            Debug.Log("Error : Bad code for iterating turn order");
        }

        PlayerCharacter currentCharacter = this.playerCharacters[currentCharacterClassID];
        currentCharacter.ResetTurnState();

        //if we don't own that char, swap player turn
        if (this.playerTurn != characterOwners[currentCharacterClassID])
        {
            this.SwapPlayerTurn();
        }
        
        this.assignControlModeToActivePlayer(this.playerTurn, ControlMode.move);
    }

    [Server]
    private void assignControlModeToActivePlayer(int playerTurn, ControlMode activeMode)
    {
        List<NetworkConnectionToClient> connections = new();
        NetworkServer.connections.Values.CopyTo(connections);
        NetworkConnectionToClient playingClient = null;
        NetworkConnectionToClient idleClient = null;
        bool excessClients = false;
        foreach (NetworkConnectionToClient conn in connections)
        {
            if (conn.identity.GetComponent<PlayerController>().playerID == playerTurn)
            {
                if (playingClient != null)
                    excessClients = true;
                playingClient = conn;
            }
            else
            {
                if (idleClient != null)
                    excessClients = true;
                idleClient = conn;
            }

        }

        if (excessClients)
            Debug.Log("Watch out, it would appear there are more than 2 connected clients...");

        this.inputHandler.RpcSetControlModeOnClient(playingClient, activeMode);
        this.inputHandler.RpcSetControlModeOnClient(idleClient, ControlMode.none);
    }

    [Server]
    private void SwapPlayerTurn()
    {
        if (this.playerTurn == 0)
        {
            this.playerTurn = 1;
        }
        else
        {
            this.playerTurn = 0;
        }
    }

    //Used by End turn button
    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        this.NextTurn();
    }

    [Server]
    public void NextTurn()
    {
        switch (this.currentPhase)
        {
            case GamePhase.waitingForClient:
                throw new Exception("You shouldn't be able to end turn while waiting for client...");
            case GamePhase.draft:
                //if(this.!AllCharactersAreDrafted())
                //    this.SwapPlayerTurn();
                //else
                //    this.SetPhase(GameMode.characterPlacement);
                break;
            case GamePhase.characterPlacement:
                if (!AllHisCharactersAreOnBoard(this.OtherPlayer(playerTurn)))
                {
                    this.SwapPlayerTurn();
                }

                if (AllCharactersAreOnBoard())
                {
                    this.SetPhase(GamePhase.gameplay);                    
                }
                break;
            case GamePhase.gameplay:
                this.NextCharacterTurn();
                break;
            case GamePhase.treasureDraft:
                this.SwapPlayerTurn();
                break;
            case GamePhase.treasureEquip:
                this.SwapPlayerTurn();
                break;
        }
    }

    [Server]
    private void SetPhase(GamePhase newPhase)
    {
        this.currentPhase = newPhase;

        switch(newPhase)
        {
            case GamePhase.characterPlacement:
                this.InitCharacterPlacementMode();
                break;
            case GamePhase.gameplay:
                this.InitGameplayMode();
                break;
        }
    }
    
    [Server]
    private void InitCharacterPlacementMode()
    {
        Debug.Log("Initializing character placement mode");
        this.playerTurn = 0;
        this.inputHandler.SetControlModeOnAllClients(ControlMode.characterPlacement);
    }

    [Server]
    public void InitGameplayMode()
    {
        Debug.Log("Initializing gameplay mode");
        this.turnOrderIndex = 0;
        this.playerTurn = 0;

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = -1;
        int i = 0;
        foreach (int classID in this.sortedTurnOrder.Values)
        {
            if (i == this.turnOrderIndex)
            {
                currentCharacterClassID = classID;
            }
            i++;
        }
        if (currentCharacterClassID == -1)
        {
            Debug.Log("Error : Bad code for iterating turn order");
        }

        //if we don't own that char, swap player turn
        if (this.playerTurn != characterOwners[currentCharacterClassID])
        {
            this.SwapPlayerTurn();
        }

        this.assignControlModeToActivePlayer(this.playerTurn, ControlMode.move);
        this.RpcOnInitGameplayMode();
    }
    #endregion

    #region Utility

    public bool ItsMyTurn()
    {
        return this.LocalPlayer.playerID == this.playerTurn;
    }

    public bool ItsThisPlayersTurn(int playerID)
    {
        return playerID == this.playerTurn;
    }

    public bool ItsThisCharactersTurn(int classID)
    {
        return this.ClassIdForTurn() == classID;
    }

    public bool HeOwnsThisCharacter(int playerID, int classID)
    {
        if (this.characterOwners[classID] == playerID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AllCharactersAreOnBoard() { 
        foreach (int classID in this.sortedTurnOrder.Values)
        {

            //works because PlayerCharacter is only created when its placed on board
            if (!this.playerCharacters.ContainsKey(classID))
            {
                return false;
            }
        }
        return true;
    }

    public bool ThisCharacterIsPlacedOnBoard(int classID)
    {
        return this.playerCharacters.ContainsKey(classID);
    }

    public bool AllHisCharactersAreOnBoard(int playerID) {
        foreach (int classID in this.sortedTurnOrder.Values)
        {
            if (HeOwnsThisCharacter(playerID, classID) &&
                !ThisCharacterIsPlacedOnBoard(classID))
            {
                return false;
            }
        }
        return true;
    }
    
    //return -1 if no character matches turn order index
    public int ClassIdForTurn(int turnOrderIndex = -1)
    {
        //finds prefab ID for character whose turn it is
        int currentTurnOrderIndex = (turnOrderIndex == -1 ? this.turnOrderIndex : turnOrderIndex);
        int sortedTurnOrderIndex = 0;
        foreach (int classID in this.sortedTurnOrder.Values)
        {
            if (sortedTurnOrderIndex == currentTurnOrderIndex)
            {
                return classID;
            }
            else
            {
                sortedTurnOrderIndex++;
            }

        }

        return -1;
    }

    private int OtherPlayer (int playerID)
    {
        if (playerID == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    #endregion

    private void Update()
    {
        if (this.waitingForClientSpawns && isServer)
        {

            if (NetworkManager.singleton.numPlayers == 2 && Map.Singleton.hexesSpawnedOnClient)
            {
                this.waitingForClientSpawns = false;
                this.CmdStartPlaying();
            }

        }
    }
}