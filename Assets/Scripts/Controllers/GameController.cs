using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

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
    public List<PlayerController> playerControllers = new();
    List<int> charactersInstantiantedOnRemote;

    private string openedTreasureID;
    private int treasureOpenedByPlayerID;

    private List<string> alreadyDraftedEquipmentIDs = new();
    public bool StartZonesCleared { get; internal set; }
    public Dictionary<int, bool> ClearedStartZones { get; private set; }

    public int DiceRollPopupPlayedOnClients { get; internal set; }

    public int SceneFadeOutOnClients { get; internal set; }

    //Only exist in some scenes, so they need to plug themselves here in their own Awake()
    //dont try to set them here, too tricky to wait for them before handling scene initialization
    public MapInputHandler mapInputHandler;
    public DraftUI draftUI;
    public EquipmentDraftUI equipmentDraftUI;

    #endregion

    #region Synced vars

    //maps classID to playerID
    private readonly SyncDictionary<int, int> draftedCharacterOwners = new();

    public SyncDictionary<int, int> DraftedCharacterOwners => this.draftedCharacterOwners;

    //maps classID to PlayerCharacter
    private readonly SyncDictionary<int, uint> playerCharactersNetIDs = new();
    public SyncDictionary<int, uint> PlayerCharactersNetIDs => this.playerCharactersNetIDs;

    private readonly Dictionary<int, PlayerCharacter> playerCharactersByID = new();
    public Dictionary<int, PlayerCharacter> PlayerCharactersByID => this.playerCharactersByID;

    //maps character initiative to classID
    private readonly SyncIDictionary<float, int> sortedTurnOrder = new(new SortedList<float, int>());
    public SyncIDictionary<float, int> SortedTurnOrder => this.sortedTurnOrder;

    [SyncVar]
    private GamePhaseID currentPhaseID;
    public GamePhaseID CurrentPhaseID { get => this.currentPhaseID;}
    [Server]
    public void SetCurrentPhaseID(GamePhaseID value)
    {
        this.currentPhaseID = value;
    }

    [SyncVar]
    private int currentRound;
    public int CurrentRound { get => this.currentRound;}
    [Server]
    public void SetCurrentRound(int value)
    {
        this.currentRound = value;
    }

    private readonly SyncDictionary<int, int> playerScores = new();

    //sceneName => awokenState
    private readonly SyncDictionary<string, bool> remoteAwokenScenes = new();
    private readonly SyncDictionary<string, bool> hostAwokenScenes = new();

    //Needs to be at bottom of syncvars since it updates UI based on state (order of syncvars determines execution order)
    //index of currently playing character during gameplay phase
    [SyncVar(hook = nameof(OnTurnOrderIndexChanged))]
    private int turnOrderIndex;
    public int TurnOrderIndex { get => this.turnOrderIndex; }

    [Server]
    public void SetTurnOrderIndex(int value)
    {
        this.turnOrderIndex = value;
    }

    //Needs to be at bottom of syncvars since it updates UI based on state (order of syncvars determines execution order)
    //currently playing playerID
    [SyncVar]
    private int playerTurn;

    public int PlayerTurn { get => this.playerTurn;}

    #endregion

    #region Startup

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);        
    }

    private void Awake()
    {
        //Debug.Log("Gamecontroller awoken");
        if (GameController.Singleton != null)
        {
            //Debug.Log("Destroying old Gamecontroller to avoid duplicate");
            Destroy(GameController.Singleton.gameObject);
        }
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
        this.PlayerCharactersNetIDs.Callback += OnPlayerCharactersNetIDsChange;
        foreach (KeyValuePair<int, uint> kvp in PlayerCharactersNetIDs)
            OnPlayerCharactersNetIDsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        this.SetPhase(new WaitingForClientPhase());
        this.turnOrderIndex = -1;
        this.SetPlayerTurn(-1);
        this.currentRound = -1;
        this.charactersInstantiantedOnRemote = new();
        this.treasureOpenedByPlayerID = -1;
        this.StartZonesCleared = false;
        this.ClearedStartZones = new();
        this.DiceRollPopupPlayedOnClients = 0;
    }

    #endregion

    #region Scene management
    [Server]
    public void ChangeToScene(string newSceneName)
    {
        //Debug.LogFormat("Changing to scene {0}", newSceneName);
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
        Debug.Log("Waiting for scene to Awake on all clients");
        while (!SceneAwokenOnAllClients(sceneName))
        {
            //Debug.Log("Waiting for scene to Awake on all clients");
            yield return null;
        }

        Debug.Log("Scene awoke on all clients, initing phase");
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

    [Command(requiresAuthority = false)]
    public void CmdNotifyCharacterInstantiatedOnRemote(int classID)
    {
        this.charactersInstantiantedOnRemote.Add(classID);
    }
    #endregion

    #region Commands/Server

    //Main game tick
    //Called at end of every turn for all gamemodes
    [Server]
    public void NextTurn()
    {
        this.currentPhaseObject.Tick();
    }

    [Command(requiresAuthority = false)]
    public void CmdEndMyTurn(NetworkConnectionToClient sender = null)
    {
        int senderPlayerID = sender.identity.GetComponent<PlayerController>().playerID;
        if (!this.ItsThisPlayersTurn(senderPlayerID))
        {
            Debug.LogFormat("Player {0} attempted to end turn while it isn't his turn. Ignoring.", senderPlayerID);
            return;
        }

        if (!(this.currentPhaseID == GamePhaseID.gameplay))
        {
            Debug.LogFormat("Player {0} attempted to end turn outside of gameplay phase. Ignoring.", senderPlayerID);
            return;
        }
        this.NextTurn();
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
                newPhase.Init("Placement " + this.CurrentRound, this);
                break;
            case GamePhaseID.gameplay:
                newPhase.Init("Gameplay " + this.CurrentRound, this);
                break;
            case GamePhaseID.equipmentDraft:
                newPhase.Init("Equipment draft" + this.CurrentRound, this);
                break;
        }
    }

    [Server]
    public void SwapPlayerTurn()
    {
        if (this.PlayerTurn == 0)
            this.SetPlayerTurn(1);
        else
            this.SetPlayerTurn(0);
    }

    [Server]
    public void AssignControlModesForNewTurn(int playerTurn, ControlMode activePlayerMode, int activeCharacterID)
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

        Hex activeCharacterHex = Map.GetHex(Map.Singleton.hexGrid, Map.Singleton.characterPositions[activeCharacterID]);
        this.mapInputHandler.TargetRpcSetControlMode(playingClient, activePlayerMode, activeCharacterHex);
        this.mapInputHandler.TargetRpcSetControlMode(idleClient, ControlMode.none, null);
    }

    [Server]
    public void DraftCharacter(int draftedByPlayerID, int classID)
    {
        if (this.CharacterHasBeenDrafted(classID))
        {
            Debug.Log("Character was already drafted.");
            return;
        }
            
        this.DraftedCharacterOwners.Add(classID, draftedByPlayerID);
    }

    [Server]
    internal void CharacterCrowned()
    {
        if (this.AllPlayersAssignedKings())
        {
            //wait a little just for less abrupt transition
            //scene changed callback will set phase once all clients have loaded scene
            this.StartCoroutine(this.WaitALittle(0.8f, () => { this.ChangeToScene("MainGame"); }));            
        }
    }

    private IEnumerator WaitALittle(float seconds, Action doAfter)
    {
        yield return new WaitForSeconds(seconds);
        doAfter();
    }

    [Server]
    public void AddCharToTurnOrder(int classID)
    {
        float initiative = this.playerCharactersByID[classID].CurrentStats.initiative;
        if (Utility.DictContainsValue(this.SortedTurnOrder, classID))
        {
            Debug.Log("Error : Character is already in turnOrder.");
            return;
        }
        this.SortedTurnOrder.Add(initiative, classID);
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

 
    [Server]
    internal void ClearTurnOrder()
    {
        this.sortedTurnOrder.Clear();
    }

    [Server]
    internal void ClearPlayerCharacters()
    {
        this.playerCharactersNetIDs.Clear();
        this.PlayerCharactersByID.Clear();
    }

    [Server]
    internal void SetTreasureOpenedByPlayerID(int playerID)
    {
        this.treasureOpenedByPlayerID = playerID;
    }

    //-1 if unopened
    [Server]
    internal int GetTreasureOpenedByPlayerID()
    {
        return this.treasureOpenedByPlayerID;
    }

    [Server]
    internal string GetTreasureIDForRound()
    {
        return this.openedTreasureID;
    }

    //-1 if unopened
    [Server]
    internal void SetOpenedTreasureID(string equipmentID)
    {
        this.openedTreasureID = equipmentID;
    }

    [Server]
    internal void AddAlreadyDraftedEquipmentID(string equipmentID)
    {
        this.alreadyDraftedEquipmentIDs.Add(equipmentID);
    }

    [Server]
    internal bool AlreadyDraftedEquipmentID(string equipmentID)
    {
        return this.alreadyDraftedEquipmentIDs.Contains(equipmentID);
    }

    public List<string> RollNewEquipmentIDs(uint numToRoll)
    {
        if (EquipmentDataSO.Singleton.GetEquipmentIDs().Count < numToRoll + this.AlreadyDraftedEquipmentCount())
            throw new System.Exception("Not enough equipments available for draft... fix");

        List<string> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;

            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); } while (this.AlreadyDraftedEquipmentID(newEquipmentID));
            rolledIDs.Add(newEquipmentID);
            this.AddAlreadyDraftedEquipmentID(newEquipmentID);
        }
        return rolledIDs;
    }

    [Server]
    internal int AlreadyDraftedEquipmentCount()
    {
        return this.alreadyDraftedEquipmentIDs.Count;
    }

    [Command(requiresAuthority = false)]
    internal void CmdNotifyStartZonesCleared(int clearedForplayerID)
    {
        this.ClearedStartZones[clearedForplayerID] = true;
        foreach (int playerID in this.playerControllers.Select(player => player.playerID).ToList())
        {
            if(!ClearedStartZones.ContainsKey(playerID) || ClearedStartZones[playerID] == false)
            {
                return;
            }
        }
        this.StartZonesCleared = true;
    }

    [Command(requiresAuthority = false)]
    public void CmdConcedRound(NetworkConnectionToClient sender = null)
    {
        int concededByPlayerID = sender.identity.GetComponent<PlayerController>().playerID;
        Debug.LogFormat("Player {0} conceded round", concededByPlayerID);
        GameController.Singleton.EndRound(concededByPlayerID);
    }

    [Server]
    public void SetPlayerTurn(int playerID)
    {
        this.playerTurn = playerID;
        if (this.IsValidPlayerID(playerID))
        {
            this.RpcOnPlayerTurnChanged(playerID);
        }
    }

    [Command(requiresAuthority = false)]
    internal void CmdDiceRollPlayed()
    {
        this.DiceRollPopupPlayedOnClients++;
    }

    [Command(requiresAuthority = false)]
    internal void CmdNotifySceneFadeOutComplete()
    {
        this.SceneFadeOutOnClients++;
    }
    #endregion

    #region Callbacks
    //callback for syncing list of active characters
    private void OnPlayerCharactersNetIDsChange(SyncIDictionary<int, uint>.Operation op, int key, uint netidArg)
    {
        switch (op)
        {
            case SyncDictionary<int, uint>.Operation.OP_ADD:
                this.PlayerCharactersByID[key] = null;
                if (NetworkClient.spawned.TryGetValue(netidArg, out NetworkIdentity netidObject))
                {
                    this.PlayerCharactersByID[key] = netidObject.gameObject.GetComponent<PlayerCharacter>();
                    if(!isServer)
                        this.CmdNotifyCharacterInstantiatedOnRemote(key);
                }
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
        while (this.PlayerCharactersByID[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
            {
                this.PlayerCharactersByID[key] = identity.gameObject.GetComponent<PlayerCharacter>();
                if (!isServer)
                    this.CmdNotifyCharacterInstantiatedOnRemote(key);
            }
        }
    }
    #endregion

    #region Rpcs

    [ClientRpc]
    public void RpcSetupKingSelection()
    {
        List<int> kingCandidates = new();
        foreach (KeyValuePair<int, int> entry in this.DraftedCharacterOwners)
        {
            if (entry.Value == this.LocalPlayer.playerID)
                kingCandidates.Add(entry.Key);
        }

        this.draftUI.SetupKingSelection(kingCandidates);
    }

    [ClientRpc]
    public void RpcLoopMenuSongs()
    {
        AudioManager.Singleton.LoopMenuSongs();
    }

    [ClientRpc]
    public void RpcLoopGameplaySongs()
    {
        AudioManager.Singleton.LoopGameplaySongs();
    }

    public void TriggerNewCharTurnAnimation(int classID)
    {
        foreach (PlayerController player in this.playerControllers) {

            this.TargetRpcNewCharTurn(player.connectionToClient, this.HeOwnsThisCharacter(player.playerID, classID), classID);
        }
    }

    [TargetRpc]
    public void TargetRpcNewCharTurn(NetworkConnectionToClient target, bool yourCharacter, int classID)
    {
        if (CharacterTurnPopup.Singleton != null)
            CharacterTurnPopup.Singleton.TriggerAnimation(classID, yourCharacter);
    }

    #endregion

    #region Events
    [ClientRpc]
    public void RpcInitTurnOrderHud(List<TurnOrderSlotInitData> slotData, List<int> sortedTurnOrder)
    {
        TurnOrderHUD.Singleton.InitSlots(slotData, sortedTurnOrder);
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
    [ClientRpc]
    private void RpcOnPlayerTurnChanged(int newPlayerID)
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
        return this.LocalPlayer.playerID == this.PlayerTurn;
    }

    public bool ItsThisPlayersTurn(int playerID)
    {
        return playerID == this.PlayerTurn;
    }

    public bool ItsThisCharactersTurn(int classID)
    {
        return this.GetCharacterIDForTurn() == classID;
    }

    public bool HeOwnsThisCharacter(int playerID, int classID)
    {
        if (!this.DraftedCharacterOwners.ContainsKey(classID))
        {
            Debug.Log("Attempting to determine ownership of character that can't be found in drafted char list.");
            return false;
        }
            
        if (this.DraftedCharacterOwners[classID] == playerID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //note that characters are created as they are placed on board
    public bool AllCharactersPlaced() { 
        foreach (int classID in this.DraftedCharacterOwners.Keys)
        {
            
            if (!Map.Singleton.characterPositions.ContainsKey(classID))
            {
                return false;
            }
        }
        return true;
    }

    public bool ThisCharacterIsPlacedOnBoard(int classID)
    {
        return Map.Singleton.characterPositions.ContainsKey(classID);
    }

    public bool AllHisCharactersAreOnBoard(int playerID) {
        foreach (int classID in this.draftedCharacterOwners.Keys)
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
    public int GetCharacterIDForTurn(int turnOrderIndexArg = -1)
    {
        //finds prefab ID for character whose turn it is
        int turnOrderIndexToGet = (turnOrderIndexArg == -1 ? this.TurnOrderIndex : turnOrderIndexArg);
        int sortedTurnOrderIndex = 0;
        foreach (int classID in this.SortedTurnOrder.Values)
        {
            if (sortedTurnOrderIndex == turnOrderIndexToGet)
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
        if (this.DraftedCharacterOwners.Count == this.defaultNumCharsPerPlayer * 2)
            return true;
        else
            return false;
    }

    internal bool CharacterHasBeenDrafted(int classID)
    {
        return this.DraftedCharacterOwners.ContainsKey(classID);
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

    [Server]
    internal bool AllCharactersInstantiatedOnClients()
    {
        foreach (int draftedCharacterID in this.draftedCharacterOwners.Keys)
        {
            if (!this.PlayerCharactersByID.ContainsKey(draftedCharacterID) || this.PlayerCharactersByID[draftedCharacterID] == null)
            {
                return false;
            }

            if (!this.charactersInstantiantedOnRemote.Contains(draftedCharacterID))
                return false;
        }
        return true;
    }

    public List<PlayerCharacter> GetCharactersOwnedByPlayer(int playerID)
    {
        List<PlayerCharacter> ownedCharacters = new();
        foreach(PlayerCharacter character in this.PlayerCharactersByID.Values)
        {
            if (character.OwnerID == playerID)
                ownedCharacters.Add(character);
        }
        return ownedCharacters;
    }

    public List<int> GetCharacterIDsOwnedByPlayer(int playerID)
    {
        List<int> ownedCharacterIDs = new();
        foreach (var (charID, ownedByplayerID) in this.draftedCharacterOwners)
        {
            if (ownedByplayerID == playerID)
                ownedCharacterIDs.Add(charID);
        }
        return ownedCharacterIDs;
    }

    //returns owner ID
    //returns -1 if none
    internal int AKingIsDead()
    {
        foreach (PlayerCharacter character in this.playerCharactersByID.Values)
        {
            if (character.IsDead && character.IsKing)
                return character.OwnerID;
        }
        return -1;
    }

    private bool IsValidPlayerID(int playerID)
    {
        if (playerID < 0 || playerID > 1)
            return false;
        else
            return true;
    }

    #endregion
}