using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

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

    public GameObject[] AllPlayerCharPrefabs = new GameObject[10];

    //Todo: spawn at runtime to allow gaining new slots for clone or losing slots for amalgam
    //todo : move to PlayerController
    public List<CharacterSlotUI> characterSlots = new();
    #endregion

    #region Static vars
    public static GameController Singleton { get; private set; }
    #endregion

    #region Runtime vars
    //Maps classID to CharacterClass
    public Dictionary<int, CharacterClass> AllClasses { get; set; }
    public PlayerController LocalPlayer { get; set; }
    private readonly List<TurnOrderSlotUI> turnOrderSlots = new();

    private bool waitingForClientSetup;
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

    [SyncVar(hook = nameof(OnGameModeChanged))]
    public GameMode currentGameMode;
    #endregion

    #region Startup

    private void Awake()
    {
        Singleton = this;
        this.waitingForClientSetup = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.playerCharactersNetIDs.Callback += OnPlayerCharactersNetIDsChange;
        // Process initial SyncDictionary payload
        foreach (KeyValuePair<int, uint> kvp in playerCharactersNetIDs)
            OnPlayerCharactersNetIDsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);

        sortedTurnOrder.Callback += OnTurnOrderChanged;
        foreach (KeyValuePair<float, int> kvp in sortedTurnOrder)
            OnTurnOrderChanged(SyncDictionary<float, int>.Operation.OP_ADD, kvp.Key, kvp.Value);

        this.AllClasses =  CharacterClass.DefineClasses();

        Map.Singleton.Initialize();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        this.currentGameMode = GameMode.waitingForClient;
        this.turnOrderIndex = -1;
        this.playerTurn = -1;
    }

    public void Start()
    {

    }

    #endregion

    #region Callbacks
    //callback for turn order bar contents
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

    //callback turn order progression
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
            if(this.currentGameMode == GameMode.gameplay)
            {
                this.SetActiveGameplayButtons(true);
            }
        }
        else
        {
            //todo : display "Waiting for other player" msg            
            this.endTurnButton.SetActive(false);
            if (this.currentGameMode == GameMode.gameplay)
            {
                this.SetActiveGameplayButtons(false);
            }
        }
    }

    //callback for gamemode UI
    [Client]
    private void OnGameModeChanged(GameMode oldPhase, GameMode newPhase)
    {
        this.phaseLabel.text = newPhase.ToString();
    }

    //callback for list of active characters
    [Client]
    private void OnPlayerCharactersNetIDsChange(SyncIDictionary<int, uint>.Operation op, int key, uint netidArg)
    {
        switch (op)
        {
            case SyncDictionary<int, uint>.Operation.OP_ADD:
                // entry added
                //Debug.Log("Callback for character being added to Gamecontroller main list has been called.");
                //Debug.LogFormat("Adding character with key {0}", key);
                this.playerCharacters[key] = null;

                if (NetworkClient.spawned.TryGetValue(netidArg, out NetworkIdentity netidObject))
                {
                    //Debug.LogFormat("Actually adding character with key {0}", key);
                    this.playerCharacters[key] = netidObject.gameObject.GetComponent<PlayerCharacter>();
                }
                else
                {
                    //Debug.LogFormat("Couldnt find character with key {0}, calling coroutine", key);

                    StartCoroutine(PlayerFromNetIDCoroutine(key, netidArg));
                }
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
            {
                //Debug.LogFormat("Actually adding character with key {0} from coroutine", key);

                this.playerCharacters[key] = identity.gameObject.GetComponent<PlayerCharacter>();
            }
        }
    }

    [ClientRpc]
    private void RpcResetActiveGameplayButtons()
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
            this.setInteractableGameplayButtons(true);
        }
    }

    [Client]
    private void setInteractableGameplayButtons(bool state)
    {
        this.moveButton.GetComponent<Button>().interactable = state;
        this.attackButton.GetComponent<Button>().interactable = state;
    }

    [Client]
    private void SetActiveGameplayButtons(bool state)
    {
        this.moveButton.SetActive(state);
        this.attackButton.SetActive(state);
    }

    [TargetRpc]
    public void RpcGrayOutMoveButton(NetworkConnectionToClient target)
    {
        this.moveButton.GetComponent<Button>().interactable = false;        
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

    #region Commands

    //ACTUAL GAME START once everything is ready on client
    [Command(requiresAuthority = false)]
    private void CmdStartPlaying()
    {        
        this.currentGameMode = GameMode.characterPlacement;
        this.playerTurn = 0;
        //this.RpcActivateEndTurnButton();
    }

    [Server]
    private void NextCharacterTurn()
    {
        Map.Singleton.RpcClearUIState();

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
        currentCharacter.NewTurn();

        //if we don't own that char, swap player turn
        if (this.playerTurn != characterOwners[currentCharacterClassID])
        {
            this.SwapPlayerTurn();
        }

        this.RpcResetInteractableGameplayButtons();
    }

    public bool CanIMoveThisCharacter(int classID, int playerID = -1)
    {
        if(playerID == -1)
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
        this.EndTurn();
    }

    [Server]
    public void EndTurn()
    {
        switch (this.currentGameMode)
        {
            case GameMode.draft:
                this.SwapPlayerTurn();
                break;
            case GameMode.characterPlacement:
                if (!AllHisCharactersAreOnBoard(this.OtherPlayer(playerTurn)))
                {
                    this.SwapPlayerTurn();
                }

                if (AllCharactersAreOnBoard())
                {
                    this.SetPhase(GameMode.gameplay);                    
                }
                break;
            case GameMode.gameplay:
                this.NextCharacterTurn();
                break;
            case GameMode.treasureDraft:
                this.SwapPlayerTurn();
                break;
            case GameMode.treasureEquip:
                this.SwapPlayerTurn();
                break;
        }
    }

    [Server]
    private void SetPhase(GameMode newPhase)
    {
        this.currentGameMode = newPhase;

        switch(newPhase)
        {
            case GameMode.gameplay:
                this.InitGameplayMode();
                break;
        }
    }

    [Command(requiresAuthority = false)]
    public void InitGameplayMode()
    {
        Debug.Log("Resetting turn");
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

        this.RpcResetActiveGameplayButtons();
    }

    [Command(requiresAuthority = false)]
    public void CmdAddCharToTurnOrder(int ownerPlayerIndex, int initiative, int classID)
    {
        if (Utility.DictContainsValue(GameController.Singleton.sortedTurnOrder, classID))
        {
            //Todo add support for this
            Debug.Log("Character is already in turnOrder, use CmdUpdateTurnOrder instead.");
            return;
        }
        this.characterOwners.Add(classID, ownerPlayerIndex);

        //throws callback to update UI
        this.sortedTurnOrder.Add(initiative, classID);
    }

    #endregion

    #region Utility

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
    private int ClassIdForPlayingCharacter(int playingCharacterIndex = -1)
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
        if (isServer && this.waitingForClientSetup)
        {

            if (Map.Singleton.hexesSpawnedOnClient && NetworkManager.singleton.numPlayers == 2)
            {
                this.waitingForClientSetup = false;
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