using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;


//RTODO: Split UI out, all current UI callbacks should only trigger an event that is listened to by actual UI classes
//RTODO: review game progression mechanic, consider using events on phase/turn changes to setup UI elements (make buttons clickable, hexes draggable, etc) without depending on them
//RTODO: Utility functions should be moved to class that holds relevant data so that gamecontroller is no longer abused as global storage, perhaps CharacterDataSO scriptable obj should serve as global data when actually required
public class GameController : NetworkBehaviour
{
    #region Editor vars
    //EDITOR
    [SerializeField]
    private TextMeshProUGUI phaseLabel;

    [SerializeField]
    private GameObject endTurnButton;

    [SerializeField]
    private GameObject moveButton;

    [SerializeField]
    private GameObject attackButton;

    [SerializeField]
    private GameObject turnOrderSlotPrefab;

    [SerializeField]
    private GameObject turnOrderBar;

    [SerializeField]
    private MapInputHandler inputHandler;

    //Must be ordered in editor by classID
    public GameObject[] AllPlayerCharPrefabs = new GameObject[10];

    public uint defaultNumCharsPerPlayer = 3;

    #endregion

    #region Static vars
    public static GameController Singleton { get; private set; }
    #endregion

    #region Runtime vars
    //Maps classID to CharacterClass
    public Dictionary<int, CharacterClass> CharacterClassesByID { get; set; }
    public PlayerController LocalPlayer { get; set; }
    private readonly List<TurnOrderSlotUI> turnOrderSlots = new();

    private bool waitingForClientSpawns;

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

    //stores index of currently playing character during gameplay phase
    [SyncVar(hook = nameof(OnTurnOrderIndexChanged))]
    public int turnOrderIndex;

    //stores currently playerID
    [SyncVar(hook = nameof(OnPlayerTurnChanged))]
    public int playerTurn;

    [SyncVar(hook = nameof(OnGamePhaseChanged))]
    public GamePhase currentPhase;
    #endregion

    #region Startup

    private void Awake()
    {
        Singleton = this;

        //ensure array is sorted by classID
        //Array.Sort<GameObject>(AllPlayerCharPrefabs,(p1, p2) => { return p1.GetComponent<PlayerCharacter>().charClass.classID.CompareTo(p2.GetComponent<PlayerCharacter>().charClass.classID);});

        this.waitingForClientSpawns = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //setup sync dict callbacks to sync actual objects from netids
        this.playerCharactersNetIDs.Callback += OnPlayerCharactersNetIDsChange;
        foreach (KeyValuePair<int, uint> kvp in playerCharactersNetIDs)
            OnPlayerCharactersNetIDsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);

        sortedTurnOrder.Callback += OnTurnOrderChanged;
        foreach (KeyValuePair<float, int> kvp in sortedTurnOrder)
            OnTurnOrderChanged(SyncDictionary<float, int>.Operation.OP_ADD, kvp.Key, kvp.Value);

        this.CharacterClassesByID =  CharacterClass.DefineClasses();
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
    //callback for turn order bar UI contents
    [Client]
    private void OnTurnOrderChanged(SyncIDictionary<float, int>.Operation op, float key, int value)
    {
        switch (op)
        {
            case SyncIDictionary<float, int>.Operation.OP_ADD:
                // entry added
                //Debug.LogFormat("Adding {0} with priority {1}", AllPlayerCharPrefabs[value].name, key);
                GameObject slot = Instantiate(this.turnOrderSlotPrefab, this.turnOrderBar.transform);
                turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());
                this.UpdateTurnOrderSlotsUI();
                break;

            case SyncIDictionary<float, int>.Operation.OP_SET:
                // entry changed
                break;

            case SyncIDictionary<float, int>.Operation.OP_REMOVE:
                // entry removed
                //Debug.LogFormat("Removing {0} with priority {1}", AllPlayerCharPrefabs[value].name, key);
                foreach (TurnOrderSlotUI currentSlot in turnOrderSlots)
                {
                    if (currentSlot.holdsCharacterWithClassID == value)
                    {
                        //Debug.LogFormat("Destroying slot with {0}", AllPlayerCharPrefabs[value].name);
                        this.turnOrderSlots.Remove(currentSlot);
                        Destroy(currentSlot.gameObject);
                        break;
                    }
                }
                this.UpdateTurnOrderSlotsUI();
                break;

            case SyncIDictionary<float, int>.Operation.OP_CLEAR:
                // Dictionary was cleared
                break;
        }
    }

    //callback turn order UI progression
    [Client]
    private void OnTurnOrderIndexChanged(int prevTurnIndex, int newTurnIndex)
    {
        //Debug.Log("OnCharacterTurnChanged");

        //finds prefab ID for character whose turn it is
        int newTurnCharacterId = this.ClassIdForPlayingCharacter();

        //highlights turnOrderSlotUI for currently playing character
        int i = 0;
        foreach (TurnOrderSlotUI slot in turnOrderSlots)
        {
            i++;
            if (slot.holdsCharacterWithClassID == newTurnCharacterId)
            {
                //Debug.Log("Highlighting slot");
                slot.HighlightAndLabel(i);
            }
            else
            {
                slot.UnhighlightAndLabel(i);

            }
        }
    }

    //used to update UI from callbacks
    [Client]
    private void UpdateTurnOrderSlotsUI()
    {
        //Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach (float initiative in this.sortedTurnOrder.Keys)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            if (i >= this.turnOrderSlots.Count) return;

            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            Image slotImage = slot.GetComponent<Image>();
            int classID = this.sortedTurnOrder[initiative];
            slotImage.sprite = AllPlayerCharPrefabs[classID].GetComponent<SpriteRenderer>().sprite;
            slot.holdsCharacterWithClassID = classID;

            if (this.turnOrderIndex == i)
            {
                slot.HighlightAndLabel(i + 1);
            }
            else
            {
                slot.UnhighlightAndLabel(i + 1);
            }
            i++;
        }
    }

    //callback for player turn UI (end turn button)
    [Client]
    private void OnPlayerTurnChanged(int _, int newPlayer)
    {
        if (newPlayer == -1)
            //we havent started game yet
            return;
        if (newPlayer == this.LocalPlayer.playerID)
        {
            //todo: display "Its your turn" msg
            this.endTurnButton.SetActive(true);
            if(this.currentPhase == GamePhase.gameplay)
            {
                this.SetActiveGameplayButtons(true);
            }
        }
        else
        {
            //todo : display "Waiting for other player" msg            
            this.endTurnButton.SetActive(false);
            if (this.currentPhase == GamePhase.gameplay)
            {
                this.SetActiveGameplayButtons(false);
            }
        }
    }

    //callback for gamemode UI
    [Client]
    private void OnGamePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        this.phaseLabel.text = newPhase.ToString();
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

    [Client]
    private void SetInteractableGameplayButtons(bool state)
    {
        this.moveButton.GetComponent<Button>().interactable = state;
        this.attackButton.GetComponent<Button>().interactable = state;
        if (!state)
        {
            this.moveButton.GetComponent<Image>().color = Color.white;
            this.attackButton.GetComponent<Image>().color = Color.white;
        }
    }

    [Client]
    private void SetActiveGameplayButtons(bool state)
    {
        this.moveButton.SetActive(state);
        this.attackButton.SetActive(state);
    }


    [Client]
    public void HighlightGameplayButton(ControlMode mode)
    {
        switch (mode)
        {
            case ControlMode.move:
                this.moveButton.GetComponent<Image>().color = Color.green;
                this.attackButton.GetComponent<Image>().color = Color.white;
                break;
            case ControlMode.attack:
                this.moveButton.GetComponent<Image>().color = Color.white;
                this.attackButton.GetComponent<Image>().color = Color.green;
                break;                
        }
    }
    #endregion

    #region Rpcs
    [TargetRpc]
    public void RpcGrayOutMoveButton(NetworkConnectionToClient target)
    {
        this.moveButton.GetComponent<Button>().interactable = false;
        this.moveButton.GetComponent<Image>().color = Color.white;

    }

    [TargetRpc]
    public void RpcGrayOutAttackButton(NetworkConnectionToClient target)
    {
        this.attackButton.GetComponent<Button>().interactable = false;
        this.attackButton.GetComponent<Image>().color = Color.white;

    }

    [TargetRpc]
    public void RpcSetControlModeOnClient(NetworkConnectionToClient target, ControlMode mode)
    {
        this.inputHandler.SetControlMode(mode);
    }

    [ClientRpc]
    private void RpcActivateGameplayButtons()
    {
        if (playerTurn == this.LocalPlayer.playerID)
        {
            this.SetActiveGameplayButtons(true);
        }
    }

    [ClientRpc]
    private void RpcResetInteractableGameplayButtons()
    {
        if (playerTurn == this.LocalPlayer.playerID)
        {
            this.SetInteractableGameplayButtons(true);
        }
    }

    [ClientRpc]
    private void SetControlModeOnAllClients(ControlMode mode)
    {
        this.inputHandler.SetControlMode(mode);
    }
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
        
        this.SetControlModeOnClientsForTurn(this.playerTurn);
       
        this.RpcResetInteractableGameplayButtons();
    }

    [Server]
    private void SetControlModeOnClientsForTurn(int playerTurn)
    {
        List<NetworkConnectionToClient> connections = new();
        NetworkServer.connections.Values.CopyTo(connections);
        NetworkConnectionToClient newTurnClient = null;
        NetworkConnectionToClient otherClient = null;
        foreach (NetworkConnectionToClient conn in connections)
        {
            if (conn.identity.GetComponent<PlayerController>().playerID == playerTurn)
            {
                if (newTurnClient != null)
                    Debug.Log("Watch out, it would appear there are more than 2 connected clients...");
                newTurnClient = conn;
            }
            else
            {
                if (otherClient != null)
                    Debug.Log("Watch out, it would appear there are more than 2 connected clients...");
                otherClient = conn;
            }

        }
        this.RpcSetControlModeOnClient(newTurnClient, ControlMode.move);
        this.RpcSetControlModeOnClient(otherClient, ControlMode.none);
    }

    [Server]
    public void SwapPlayerTurn()
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
                this.initCharacterPlacementMode();
                break;
            case GamePhase.gameplay:
                this.InitGameplayMode();
                break;
        }
    }
    
    [Server]
    private void initCharacterPlacementMode()
    {
        Debug.Log("Initializing character placement mode");
        this.playerTurn = 0;
        this.SetControlModeOnAllClients(ControlMode.characterPlacement);
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

        this.SetControlModeOnClientsForTurn(this.playerTurn);
        this.RpcActivateGameplayButtons();
    }

    [Command(requiresAuthority = false)]
    public void CmdAddCharToTurnOrder(int ownerPlayerIndex, float initiative, int classID)
    {
        if (Utility.DictContainsValue(GameController.Singleton.sortedTurnOrder, classID))
        {
            //Todo add support for this
            Debug.Log("Character is already in turnOrder, use CmdUpdateTurnOrder instead.");
            return;
        }

        //todo: move to draft phase
        this.characterOwners.Add(classID, ownerPlayerIndex);

        //throws callback to update UI
        this.sortedTurnOrder.Add(initiative, classID);
    }

    #endregion

    #region Utility

    public bool CanIControlThisCharacter(int classID, int playerID = -1)
    {
        if (playerID == -1)
        {
            playerID = this.LocalPlayer.playerID;
        }
        if (this.IsItThisPlayersTurn(playerID) &&
            this.IsItThisCharactersTurn(classID) &&
            this.DoesHeOwnThisCharacter(playerID, classID))
            return true;
        else
            return false;
    }

    public GameObject GetCharPrefabWithClassID(int classID)
    {
        foreach (GameObject prefab in this.AllPlayerCharPrefabs)
        {
            if (prefab.GetComponent<PlayerCharacter>().charClassID == classID)
                return prefab;
        }
        Debug.Log("Couldn't find any prefabs with given classID");
        return null;
    }

    public bool IsItMyTurn()
    {
        return this.LocalPlayer.playerID == this.playerTurn;
    }

    public bool IsItThisPlayersTurn(int playerID)
    {
        return playerID == this.playerTurn;
    }

    public bool IsItThisCharactersTurn(int classID)
    {
        return this.ClassIdForPlayingCharacter() == classID;
    }

    public bool DoIOwnThisCharacter(int classID)
    {
        if (this.characterOwners[classID] == this.LocalPlayer.playerID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool DoesHeOwnThisCharacter(int playerID, int classID)
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
            if (!this.playerCharacters.ContainsKey(classID))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsThisCharacterPlacedOnBoard(int classID)
    {
        return this.playerCharacters.ContainsKey(classID);
    }

    public bool AllHisCharactersAreOnBoard(int playerID) {
        foreach (int classID in this.sortedTurnOrder.Values)
        {
            if (DoesHeOwnThisCharacter(playerID, classID) &&
                !IsThisCharacterPlacedOnBoard(classID))
            {
                return false;
            }
        }
        return true;
    }
    
    //return -1 if no character matches turn order index
    public int ClassIdForPlayingCharacter(int playingCharacterIndex = -1)
    {
        //finds prefab ID for character whose turn it is
        int currentTurnOrderIndex = (playingCharacterIndex == -1 ? this.turnOrderIndex : playingCharacterIndex);
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
        if (isServer && this.waitingForClientSpawns)
        {

            if (NetworkManager.singleton.numPlayers == 2 && Map.Singleton.hexesSpawnedOnClient)
            {
                this.waitingForClientSpawns = false;
                this.CmdStartPlaying();
            }

        }
    }

    //used for testing functionnalities without waiting for client setup
    public void TestButton()
    {
        //testing
        //Hex target = Map.Singleton.GetHex(2, 1);
        //target.AttackHover(true);
        //bool result = Map.Singleton.LOSReaches(Map.Singleton.GetHex(0, 0), target, 10);
        //Debug.Log(result);

    }


}