using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using UnityEngine.SceneManagement;

//RTODO: review game progression mechanic
public class GameController : NetworkBehaviour
{
    #region Editor vars
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

    //maintained on server only
    public IGamePhase currentPhaseObject;

    public MapInputHandler mapInputHandler;

    public DraftUI draftUI;

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

    [SyncVar]
    public GamePhaseID currentPhaseID;

    [SyncVar]
    public int currentRound;

    #endregion

    #region Startup

    //needs to be in start : https://mirror-networking.gitbook.io/docs/manual/components/networkbehaviour
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Awake()
    {
        if (GameController.Singleton != null)
            Destroy(GameController.Singleton.gameObject);
        GameController.Singleton = this;
        this.waitingForClientSpawns = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
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
        
        this.SetPhase(new WaitingForClientPhase());
        this.turnOrderIndex = -1;
        this.playerTurn = -1;
        this.currentRound = -1;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Draft":
                this.draftUI = GameObject.FindWithTag("DraftUI").GetComponent<DraftUI>();
                break;
            case "MainGame":
                Map.Singleton.Initialize();

                //should have been instantiated by now since this is called after awake on scene objects
                this.mapInputHandler = MapInputHandler.Singleton;
                break;
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
    public void RpcOnInitGameplayMode()
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

    #region Commands/Server

    //ACTUAL GAME START once everything is ready on client
    [Command(requiresAuthority = false)]
    private void CmdStartPlaying()
    {
        this.currentRound = 0;
        //TODO change to actual draft
        //foreach(PlayerController player in this.playerControllers)
        //{
        //    player.FakeDraft();
        //}
        //this.SetPhase(new CharacterPlacementPhase());

        this.SetPhase(new CharacterDraftPhase());
    }

    //Main game tick
    //Called at end of every turn for all gamemodes
    [Command(requiresAuthority = false)]
    public void NextTurn()
    {
        this.currentPhaseObject.Tick();
    }

    [Server]
    public void SetPhase(IGamePhase newPhase)
    {
        Debug.LogFormat("Changing to phase {0}", newPhase.ID);
        this.currentPhaseObject = newPhase;
        this.currentPhaseID = newPhase.ID;

        switch (newPhase.ID)
        {
            case GamePhaseID.waitingForClient:
                newPhase.Init("Waiting for client", this);
                break;
            case GamePhaseID.characterDraft:
                newPhase.Init("Drafting characters", this);
                break;
            case GamePhaseID.characterPlacement:
                newPhase.Init("Placement " + this.currentRound, this);
                break;
            case GamePhaseID.gameplay:
                newPhase.Init("Gameplay " + this.currentRound, this);
                break;
        }
    }

    [Server]
    public void SwapPlayerTurn()
    {
        if (this.playerTurn == 0)
            this.playerTurn = 1;
        else
            this.playerTurn = 0;
    }

    [Server]
    public void assignControlModesForNewTurn(int playerTurn, ControlMode activePlayerMode)
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

        this.mapInputHandler.RpcSetControlModeOnClient(playingClient, activePlayerMode);
        this.mapInputHandler.RpcSetControlModeOnClient(idleClient, ControlMode.none);
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

    //note that characters are created as they are placed on board
    public bool AllPlayerCharactersCreated() { 
        foreach (int classID in this.sortedTurnOrder.Values)
        {
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

    public int OtherPlayer (int playerID)
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
        //waits for all setup between phases before starting
        if (this.waitingForClientSpawns && isServer)
        {
            switch (this.currentPhaseID)
            {
                case GamePhaseID.waitingForClient:
                    if(NetworkManager.singleton.numPlayers == 2)
                    {
                        this.waitingForClientSpawns = false;
                        this.CmdStartPlaying();
                    }
                    break;
                //case GamePhaseID.characterPlacement:
                //    if (NetworkManager.singleton.numPlayers == 2 && Map.Singleton.hexesSpawnedOnClient)
                //    {
                //        this.waitingForClientSpawns = false;
                //        this.CmdStartPlaying();
                //    }
                //    break;
            }

        }
    }
}