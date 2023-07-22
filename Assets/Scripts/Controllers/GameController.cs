using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    #region Editor vars
    public uint defaultNumCharsPerPlayer = 3;

    #endregion

    #region Event vars
    [SerializeField]
    private IntGameEventSO onCharAddedToTurnOrder;

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

    public PlayerController LocalPlayer { get; set; }

    //Only filled on server
    public List<PlayerController> playerControllers = new();
    private string remoteClientLoadedScene = "";
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

    #region Commands/Server

    //Main game tick
    //Called at end of every turn for all gamemodes
    [Command(requiresAuthority = false)]
    public void NextTurn()
    {
        this.currentPhaseObject.Tick();
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoteClientFinishedLoadingScene(string sceneName)
    {
        this.remoteClientLoadedScene = sceneName;
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

        this.mapInputHandler.TargetRpcSetControlModeOnClient(playingClient, activePlayerMode);
        this.mapInputHandler.TargetRpcSetControlModeOnClient(idleClient, ControlMode.none);
    }

    [Server]
    public void CmdDraftCharacter(int draftedByPlayerID, int classID)
    {
        this.characterOwners.Add(classID, draftedByPlayerID);

        if (this.draftUI.AllCharactersHaveBeenDrafted())
        {
            Debug.Log("All chars drafted. Setting up king selection.");
            this.RpcSetupKingSelection();
        }
    }

    [Command(requiresAuthority = false)]
    internal void CmdCrownCharacter(int playerID, int classID)
    {
        if (this.AllPlayersAssignedKings())
        {
            //just change scene, scene changed callback will set phase once all clients have loaded scene
            NetworkManager.singleton.ServerChangeScene("MainGame");
        }
    }


    [Server]
    public void AddCharToTurnOrder(int classID)
    {
        float initiative = ClassDataSO.Singleton.GetClassByID(classID).stats.initiative;
        if (Utility.DictContainsValue(this.sortedTurnOrder, classID))
        {
            Debug.Log("Error : Character is already in turnOrder.");
            return;
        }
        this.sortedTurnOrder.Add(initiative, classID);
        this.RpcOnCharAddedToTurnOrder(classID);
    }

    [Server]
    private void ServerSceneInit(string sceneName)
    {
        switch (sceneName)
        {
            case "Draft":
                this.SetPhase(new CharacterDraftPhase());
                break;

            case "MainGame":
                this.SetPhase(new CharacterPlacementPhase());

                break;
        }
    }
    #endregion

    #region Callbacks
    //callback for syncing list of active characters
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
    private IEnumerator PlayerFromNetIDCoroutine(int key, uint netIdArg)
    {
        while (this.playerCharacters[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
                this.playerCharacters[key] = identity.gameObject.GetComponent<PlayerCharacter>();
        }
    }

    //Phases that require new scene are set in this callback to ensure scene gameobjects are loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogFormat("Loaded scene {0}", scene.name);

        //needed because network isn't properly setup yet, causing weird stuff
        if (scene.name == "Lobby")
            return;

        this.LocalSceneInit(scene.name);

        //rest of scene init can be handled server side or with Rpcs
        if (!isServer)
            return;

        //ensure that remote client has finished loading scene
        if (remoteClientLoadedScene == scene.name)
        {
            this.ServerSceneInit(scene.name);
        }
        else
        {
            StartCoroutine(InitSceneOnceReadyCoroutine(scene.name));
        }
    }

    private IEnumerator InitSceneOnceReadyCoroutine(string sceneName)
    {
        while (this.remoteClientLoadedScene != sceneName)
        {
            yield return null;            
        }
        this.ServerSceneInit(sceneName);
    }

    //setup scene references on all clients ASAP to avoid null refs that can occur when setting them via async Rpcs
    private void LocalSceneInit(string sceneName)
    {
        if (sceneName == "Menu")
        {
            Destroy(this.gameObject);
        }

        if (sceneName == "Draft")
            this.draftUI = GameObject.FindWithTag("DraftUI").GetComponent<DraftUI>();

        if (sceneName == "MainGame")
        {
            Debug.Log("initiliazing map input handler ref from gamecontroller on some client");
            Debug.Log(MapInputHandler.Singleton);
            this.mapInputHandler = MapInputHandler.Singleton;
            Debug.Log(this.mapInputHandler);
        }
    }

    #endregion

    #region Rpcs

    [ClientRpc]
    private void RpcSetupKingSelection()
    {
        List<int> kingCandidates = new();
        foreach (KeyValuePair<int, int> entry in this.characterOwners)
        {
            if (entry.Value == this.LocalPlayer.playerID)
                kingCandidates.Add(entry.Key);
        }

        this.draftUI.SetupKingSelection(kingCandidates);
    }

    #endregion

    #region Events
    [ClientRpc]
    private void RpcOnCharAddedToTurnOrder(int classID)
    {
        this.onCharAddedToTurnOrder.Raise(classID);
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

    [Server]
    private bool AllPlayersAssignedKings()
    {
        foreach(PlayerController player in this.playerControllers)
        {
            if (player.kingClassID == -1)
                return false;
        }
        return true;
    }
    #endregion

    private void Update()
    {
        //waits for all setup between phases before starting
        //if (this.waitingForClientSpawns && isServer)
        //    {
        //        switch (this.currentPhaseID)
        //        {
        //            case GamePhaseID.waitingForClient:
        //                if (NetworkManager.singleton.numPlayers == 2)
        //                {
        //                    this.waitingForClientSpawns = false;
        //                    this.CmdStartPlaying();
        //                }
        //                break;
        //                //case GamePhaseID.characterPlacement:
        //                //    if (NetworkManager.singleton.numPlayers == 2 && Map.Singleton.hexesSpawnedOnClient)
        //                //    {
        //                //        this.waitingForClientSpawns = false;
        //                //        this.CmdStartPlaying();
        //                //    }
        //                //    break;
        //        }
        //    }
    }

}