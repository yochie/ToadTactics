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
    [field: SerializeField]
    public uint defaultNumCharsPerPlayer { get; private set; }

    [field: SerializeField]
    public uint numEquipmentsDraftedBetweenRounds { get; private set; }

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

    [SerializeField]
    private GameEventSO onRoundWon;

    [SerializeField]
    private GameEventSO onRoundLost;

    [SerializeField]
    private GameEventSO onGameWon;

    [SerializeField]
    private GameEventSO onGameLost;

    #endregion

    #region Runtime vars

    public static GameController Singleton { get; private set; }

    public PlayerController LocalPlayer { get; set; }

    public PlayerController NonLocalPlayer { get; set; }

    //Only filled on server
    public IGamePhase currentPhaseObject;
    private int lastRoundLoserID;
    private readonly List<string> equipmentsToDraft = new();
    private readonly List<string> equipmentsToAssign = new();

    public List<PlayerController> playerControllers = new();

    //Only exist in some scenes, so they need to plug themselves here in their own Awake()
    //dont try to set them here, too tricky to wait for them before handling scene initialization
    public MapInputHandler mapInputHandler;
    public DraftUI draftUI;
    public EquipmentDraftUI equipmentDraftUI;

    #endregion

    #region Synced vars

    //maps classID to playerID
    public readonly SyncDictionary<int, int> draftedCharacterOwners = new();

    //maps classID to PlayerCharacter
    public readonly SyncDictionary<int, uint> playerCharactersNetIDs = new();
    public readonly Dictionary<int, PlayerCharacter> playerCharacters = new();

    //maps character initiative to classID
    public readonly SyncIDictionary<float, int> sortedTurnOrder = new(new SortedList<float, int>());

    [SyncVar]
    public GamePhaseID currentPhaseID;

    [SyncVar]
    public int currentRound;
    private readonly SyncDictionary<int, int> playerScores = new();

    //sceneName => awokenState
    public readonly SyncDictionary<string, bool> remoteAwokenScenes = new();
    public readonly SyncDictionary<string, bool> hostAwokenScenes = new();

    //Needs to be at bottom of syncvars since it updates UI based on state (order of syncvars determines execution order)
    //index of currently playing character during gameplay phase
    [SyncVar(hook = nameof(OnTurnOrderIndexChanged))]
    public int turnOrderIndex;

    //Needs to be at bottom of syncvars since it updates UI based on state (order of syncvars determines execution order)
    //currently playing playerID
    [SyncVar(hook = nameof(OnPlayerTurnChanged))]
    public int playerTurn;

    #endregion

    #region Startup

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Awake()
    {
        Debug.Log("Gamecontroller awoken");
        if (GameController.Singleton != null)
            Destroy(GameController.Singleton.gameObject);
        GameController.Singleton = this;
        SceneManager.sceneLoaded += this.OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;
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

    #region Scene management
    [Server]
    public void CmdChangeToScene(string newSceneName)
    {
        //mark current scene as unloaded
        string currentScene = SceneManager.GetActiveScene().name;
        this.remoteAwokenScenes[currentScene] = false;
        this.hostAwokenScenes[currentScene] = false;
        MyNetworkManager.singleton.ServerChangeScene(newSceneName);
    }

    [Command(requiresAuthority = false)]
    public void NotifySceneAwoken(bool onServer, string sceneName)
    {
        if (onServer)
            this.hostAwokenScenes[sceneName] = true;
        else
            this.remoteAwokenScenes[sceneName] = true;
    }

    private bool SceneAwokenOnAllClients(string sceneName)
    {
        if (!hostAwokenScenes.ContainsKey(sceneName) || !this.remoteAwokenScenes.ContainsKey(sceneName))
            return false;

        if (this.hostAwokenScenes[sceneName] && this.remoteAwokenScenes[sceneName])
            return true;
        else
            return false;
    }

    //TODO : not used???
    private bool SceneAwokenOnLocalClient(string sceneName)
    {
        if (this.isServer)
        {
            if (!hostAwokenScenes.ContainsKey(sceneName))
                return false;
            else
                return this.hostAwokenScenes[sceneName];
        }
        else
        {
            if (!this.remoteAwokenScenes.ContainsKey(sceneName))
                return false;
            else
                return this.remoteAwokenScenes[sceneName];
        }
    }

    //Phases that require new scene are set in this callback to ensure scene gameobjects are loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogFormat("Loaded scene {0}", scene.name);

        //needed because network isn't properly setup yet, causing weird errors
        if (scene.name == "Lobby")
            return;

        //rest of scene init can be handled server side or with Rpcs
        if (!isServer)
            return;

        //ensure that all clients has finished loading scene
        if (SceneAwokenOnAllClients(scene.name))
        {
            this.NewScenePhaseInit(scene.name);
        }
        else
        {
            StartCoroutine(InitSceneOnceAllClientsReadyCoroutine(scene.name));
        }
    }

    private IEnumerator InitSceneOnceAllClientsReadyCoroutine(string sceneName)
    {
        //Debug.Log("Waiting for scene to Awake on all clients");
        while (!SceneAwokenOnAllClients(sceneName))
        {
            yield return null;
        }

        //Debug.Log("Scene awoke, initing phase");
        this.NewScenePhaseInit(sceneName);
    }

    [Server]
    private void NewScenePhaseInit(string sceneName)
    {
        switch (sceneName)
        {
            case "Draft":
                this.SetPhase(new CharacterDraftPhase());
                break;

            case "MainGame":
                this.SetPhase(new CharacterPlacementPhase());
                break;
            case "EquipmentDraft":
                this.SetPhase(new EquipmentDraftPhase(this.lastRoundLoserID));
                break;
        }
    }
    #endregion

    #region Commands/Server

    //Main game tick
    //Called at end of every turn for all gamemodes
    [Command(requiresAuthority = false)]
    public void CmdNextTurn()
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
            case GamePhaseID.equipmentDraft:
                newPhase.Init("Equipment draft" + this.currentRound, this);
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
    public void AssignControlModesForNewTurn(int playerTurn, ControlMode activePlayerMode)
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
        this.draftedCharacterOwners.Add(classID, draftedByPlayerID);

        this.CmdNextTurn();
    }

    [Command(requiresAuthority = false)]
    internal void CmdCrownCharacter(int playerID, int classID)
    {
        if (this.AllPlayersAssignedKings())
        {
            //just change scene, scene changed callback will set phase once all clients have loaded scene
            this.CmdChangeToScene("MainGame");
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
    internal void EndRound(int looserID)
    {
        Debug.Log("Ending round.");
        int winnerID = this.OtherPlayer(looserID);
        NetworkConnectionToClient winnerClient = this.GetConnectionForPlayerID(winnerID);
        NetworkConnectionToClient looserClient = this.GetConnectionForPlayerID(looserID);

        bool gameEnded = this.WinRound(winnerID);
        if (gameEnded)
        {
            Debug.Log("Game ended.");
            this.TargetRpcOnGameWon(winnerClient);
            this.TargetRpcOnGameLost(looserClient);
        }
        else
        {
            Debug.Log("Time for another round.");
            this.TargetRpcOnRoundWon(winnerClient);
            this.TargetRpcOnRoundLost(looserClient);
        }            
    }

    [Server]
    internal void SetScore(int playerID, int score)
    {
        this.playerScores[playerID] = score;
    }

    [Server]
    private bool WinRound(int winnerID)
    {
        Debug.Log("Incrementing score for winner");
        this.playerScores[winnerID] += 1;
        this.lastRoundLoserID = this.OtherPlayer(winnerID);
        bool gameEnded = (this.playerScores[winnerID] == 2);
        return gameEnded;
    }

    internal void SetEquipmentsToDraft(List<string> equipmentIDs)
    {
        foreach(string equipmentToAdd in equipmentIDs)
        {
            this.equipmentsToDraft.Add(equipmentToAdd);
        }
    }

    internal void SetEquipmentsToAssign(List<string> equipmentIDs)
    {
        foreach (string equipmentToAdd in equipmentIDs)
        {
            this.equipmentsToAssign.Add(equipmentToAdd);
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
    #endregion

    #region Rpcs

    [ClientRpc]
    public void RpcSetupKingSelection()
    {
        List<int> kingCandidates = new();
        foreach (KeyValuePair<int, int> entry in this.draftedCharacterOwners)
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

    //TODO: ensure all handlers for these events avoid checking syncvars to ensure no race condition between syncvar propagation and RPC execution
    //TODO: caveat here is that execution order can be made deterministic by ordering syncvar declarations as long as hooks that call RPCs are defined in same file as syncvars
    //TODO: if state checks are necessary, instead use RPCs that send all required state as args
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

    [TargetRpc]
    private void TargetRpcOnGameWon(NetworkConnectionToClient target)
    {
        this.onGameWon.Raise();
    }

    [TargetRpc]
    private void TargetRpcOnGameLost(NetworkConnectionToClient target)
    {
        this.onGameLost.Raise();
    }

    [TargetRpc]
    private void TargetRpcOnRoundWon(NetworkConnectionToClient target)
    {
        this.onRoundWon.Raise();
    }

    [TargetRpc]
    private void TargetRpcOnRoundLost(NetworkConnectionToClient target)
    {
        this.onRoundLost.Raise();
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
        if (this.draftedCharacterOwners[classID] == playerID)
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
        foreach (int classID in this.draftedCharacterOwners.Keys)
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

    private bool AllPlayersAssignedKings()
    {
        foreach(PlayerController player in this.playerControllers)
        {
            if (player.kingClassID == -1)
                return false;
        }
        return true;
    }

    internal bool IsAKing(int classID)
    {
        foreach (PlayerController p in this.playerControllers)
        {
            if (p.kingClassID == classID)
                return true;
        }
        return false;
    }

    internal bool AllCharactersDrafted()
    {
        if (this.draftedCharacterOwners.Count == this.defaultNumCharsPerPlayer * 2)
            return true;
        else
            return false;
    }

    internal bool CharacterHasBeenDrafted(int classID)
    {
        return this.draftedCharacterOwners.ContainsKey(classID);
    }

    internal NetworkConnectionToClient GetConnectionForPlayerID(int playerID)
    {
        foreach(PlayerController p in this.playerControllers)
        {
            if (p.playerID == playerID)
            {
                return p.connectionToClient;
            }
        }

        Debug.Log("Couldn't find connection for requested playerID");
        return null;
    }

    internal int GetScore(int playerID)
    {
        return this.playerScores[playerID];
    }

    public bool AllEquipmentsDrafted()
    {
        foreach (string equipmentIDToDraft in this.equipmentsToDraft)
        {
            bool hasBeenDraftedByAPlayer = false;
            foreach (PlayerController player in this.playerControllers)
            {
                if (player.HasDraftedEquipment(equipmentIDToDraft))
                    hasBeenDraftedByAPlayer = true;
            }
            if (!hasBeenDraftedByAPlayer)
                return false;
        }

        return true;
    }

    [Server]
    public bool EquipmentHasBeenDrafted(string equipmentID)
    {
        foreach (PlayerController player in this.playerControllers)
        {
            if (player.HasDraftedEquipment(equipmentID))
                return true;
        }
        return false;
    }

    [Server]
    public bool AllEquipmentsAssigned()
    {
        foreach (PlayerController player in this.playerControllers)
        {
            if (!player.HasAssignedAllEquipments())
                return false;
        }
        return true;
    }

    public bool EquipmentHasBeenAssigned(string equipmentID)
    {
        foreach (PlayerController player in this.playerControllers)
        {
            if (player.HasAssignedEquipment(equipmentID))
                return true;
        }
        return false;
    }

    #endregion
}