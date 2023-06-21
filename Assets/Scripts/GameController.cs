using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.IO;
using TMPro;

public class GameController : NetworkBehaviour
{
    #region Editor vars
    //EDITOR
    [SerializeField]
    private TextMeshProUGUI phaseLabel;

    [SerializeField]
    private GameObject endTurnButton;

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
    public static Dictionary<string, CharacterClass> AllClasses { get; set; }
    #endregion 

    #region Runtime vars
    public PlayerController LocalPlayer { get; set; }
    private List<TurnOrderSlotUI> turnOrderSlots = new();
    #endregion

    #region Synced vars

    //maps prefabID to playerID
    public readonly SyncDictionary<int, int> characterOwners = new();

    //maps prefabID to PlayerCharacter
    public readonly SyncDictionary<int, uint> playerCharactersNetIDs = new();
    public readonly Dictionary<int, PlayerCharacter> playerCharacters = new();

    //maps character initiative to prefabID
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

        GameController.DefineClasses();

        Map.Singleton.Initialize();

        //testing
        //HashSet<Hex> inRange = Map.Singleton.RangeObstructed(Map.Singleton.GetHex(0, 0), 2);
        //foreach (Hex h in inRange)
        //{
        //    h.baseColor = Color.magenta;
        //}
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        //init all syncvars that aren't readonly
        this.currentGameMode = GameMode.characterPlacement;
        this.turnOrderIndex = -1;
        this.playerTurn = 0;
    }


    public void Start()
    {
        if (!IsItMyClientsTurn())
            this.endTurnButton.SetActive(false);

        //set initial turn UI for character turn 0
        //OnTurnOrderIndexChanged(-1, 0);

        //this.InitCharacterTurns();


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
                    if (currentSlot.holdsPrefabWithIndex == value)
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
        int newTurnCharacterId = this.PrefabIdForPlayingCharacter();

        //highlights turnOrderSlotUI for currently playing character
        int i = 0;
        foreach (TurnOrderSlotUI slot in turnOrderSlots)
        {
            i++;
            if (slot.holdsPrefabWithIndex == newTurnCharacterId)
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
            int prefabId = this.sortedTurnOrder[initiative];
            slotImage.sprite = AllPlayerCharPrefabs[prefabId].GetComponent<SpriteRenderer>().sprite;
            slot.holdsPrefabWithIndex = prefabId;

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
        if (newPlayer == this.LocalPlayer.playerIndex)
        {
            //todo: display "Its your turn" msg
            this.endTurnButton.SetActive(true);
        }
        else
        {
            //todo : display "Waiting for other player" msg            
            this.endTurnButton.SetActive(false);

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

    #endregion

    #region Commands
    //called by commands to modify character lists
    [Server]
    internal void AddMyChar(int playerIndex, int prefabIndex, int initiative)
    {
        this.characterOwners.Add(prefabIndex, playerIndex);

        //throws callback to update UI
        this.sortedTurnOrder.Add(initiative, prefabIndex);
    }

    //called by commands to modify turnOrderSortedPrefabIds
    [Server]
    internal void RemoveAllMyChars(int playerIndex)
    {
        List<float> ownedToRemove = new();
        foreach (int characterPrefabId in this.characterOwners.Keys)
        {
            if (this.characterOwners[characterPrefabId] == playerIndex)
            {
                List<float> turnToRemove = new();
                foreach (float initiative in this.sortedTurnOrder.Keys)
                {
                    if (this.sortedTurnOrder[initiative] == characterPrefabId)
                    {
                        turnToRemove.Add(initiative);
                    }
                }
                foreach (float toRemove in turnToRemove)
                {
                    this.sortedTurnOrder.Remove(toRemove);
                }

                ownedToRemove.Add(characterPrefabId);
            }
        }

        foreach (int toRemove in ownedToRemove)
        {
            this.characterOwners.Remove(toRemove);
        }
    }

    [Server]
    private void NextCharacterTurn()
    {
        //loops through turn order        
        this.turnOrderIndex++;
        if (this.turnOrderIndex >= this.sortedTurnOrder.Count)
            this.turnOrderIndex = 0;

        //finds character prefab id for the next turn so that we can check who owns it
        int currentCharacterPrefab = -1;
        int i = 0;
        foreach (int prefab in this.sortedTurnOrder.Values)
        {
            if (i == this.turnOrderIndex)
            {
                currentCharacterPrefab = prefab;
            }
            i++;
        }
        if (currentCharacterPrefab == -1)
        {
            Debug.Log("Error : Bad code for iterating turnOrderSortedPrefabIds");
        }

        PlayerCharacter currentPlayer = this.playerCharacters[currentCharacterPrefab];
        currentPlayer.NextTurn();

        //if we don't own that char, swap player turn
        if (this.playerTurn != characterOwners[currentCharacterPrefab])
        {
            this.SwapPlayerTurn();
        }
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

    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        this.EndTurn();
    }

    //modifies syncvars currentTurnPlayer and characterTurnOrderIndex
    [Server]
    public void EndTurn()
    {
        switch (this.currentGameMode)
        {
            case GameMode.draft:
                this.SwapPlayerTurn();
                break;
            case GameMode.characterPlacement:
                //used to make sure 
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
    private void SetPhase(GameMode phase)
    {
        this.currentGameMode = phase;

        switch(phase)
        {
            case GameMode.gameplay:
                this.InitCharacterTurns();
                break;
        }
    }

    [Command(requiresAuthority = false)]
    public void InitCharacterTurns()
    {
        Debug.Log("Resetting turn");
        this.turnOrderIndex = 0;
        this.playerTurn = 0;

        //finds character prefab id for the next turn so that we can check who owns it
        int currentCharacterPrefab = -1;
        int i = 0;
        foreach (int prefab in this.sortedTurnOrder.Values)
        {
            if (i == this.turnOrderIndex)
            {
                currentCharacterPrefab = prefab;
            }
            i++;
        }
        if (currentCharacterPrefab == -1)
        {
            Debug.Log("Error : Bad code for iterating turnOrderSortedPrefabIds");
        }

        //if we don't own that char, swap player turn
        if (this.playerTurn != characterOwners[currentCharacterPrefab])
        {
            this.SwapPlayerTurn();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddCharToTurnOrder(int ownerPlayerIndex, int initiative, int prefabID)
    {
        if (Utility.DictContainsValue(GameController.Singleton.sortedTurnOrder, prefabID))
        {
            //Todo add support for this
            Debug.Log("Character is already in turnOrder, use CmdUpdateTurnOrder instead.");
            return;
        }
        GameController.Singleton.AddMyChar(ownerPlayerIndex, prefabID, initiative);
    }

    #endregion

    #region Utility

    public bool IsItMyClientsTurn()
    {
        return this.LocalPlayer.playerIndex == this.playerTurn;
    }

    public bool IsItThisCharactersTurn(int prefabID)
    {
        return this.PrefabIdForPlayingCharacter() == prefabID;
    }

    public bool DoIOwnThisCharacter(int prefabID)
    {
        if (this.characterOwners[prefabID] == this.LocalPlayer.playerIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool DoesHeOwnThisCharacter(int playerID, int prefabID)
    {
        if (this.characterOwners[prefabID] == playerID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AllCharactersAreOnBoard() { 
        foreach (int characterPrefabID in this.sortedTurnOrder.Values)
        {
            if (!this.playerCharacters.ContainsKey(characterPrefabID))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsThisCharacterPlacedOnBoard(int characterPrefabID)
    {
        return this.playerCharacters.ContainsKey(characterPrefabID);
    }

    public bool AllHisCharactersAreOnBoard(int playerID) {
        foreach (int characterPrefabID in this.sortedTurnOrder.Values)
        {
            if (DoesHeOwnThisCharacter(playerID, characterPrefabID) &&
                !IsThisCharacterPlacedOnBoard(characterPrefabID))
            {
                return false;
            }
        }
        return true;
    }
    
    //return -1 if no character matches turn order index
    private int PrefabIdForPlayingCharacter(int playingCharacterIndex = -1)
    {
        //finds prefab ID for character whose turn it is
        int currentTurnOrderIndex = (playingCharacterIndex == -1 ? this.turnOrderIndex : playingCharacterIndex);
        int sortedTurnOrderIndex = 0;
        foreach (int prefabId in this.sortedTurnOrder.Values)
        {
            if (sortedTurnOrderIndex == currentTurnOrderIndex)
            {
                return prefabId;
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

    #region Static definitions

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private static void DefineClassesFromCSV(string statsFilename, string abilitiesFilname, string classFilename)
    {

        //Read in character stats
        string[] lines = File.ReadAllLines(statsFilename);
        int i = 0;
        foreach (string line in lines)
        {
            //process headers
            if (i == 0)
            {

            }

            string[] columns = line.Split(',');
            foreach (string column in columns)
            {
                //add data to Classes array
            }
            i++;
        }


        //define all custom abilities for each class

        //Read in character abilities and plug in custom abilities

        //Read in class meta
    }
    private static void DefineClasses()
    {
        GameController.AllClasses = new Dictionary<string, CharacterClass>();

        //Barb
        CharacterClass barbarian = new(
            name: "Barbarian",
            description: "Glass cannon, kill or get killed",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 5,
                damageType: DamageType.normal,
                moveSpeed: 3,
                initiative: 1,
                range: 1,
                damageIterations: 3),
            ability: new(
                name: "NA",
                description: "No active ability. Always attacks thrice.",
                damage: 1,
                range: 0,
                aoe: 0,
                turnDuration: 0,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Barb has no usable ability. Should probably prevent this from being called.");
                })
        );
        GameController.AllClasses.Add(barbarian.className, barbarian);

        //Cavalier
        CharacterClass cavalier = new(
            name: "Cavalier",
            description: "Rapidly reaches the backline or treasure",
            stats: new(
                maxHealth: 12,
                armor: 3,
                damage: 5,
                moveSpeed: 3,
                initiative: 2,
                range: 2,
                damageIterations: 1),
            ability: new(
                name: "Lance Throw",
                description: "Throws a lance at an enemy in a 3 tile radius, dealing damage and stunning the target until next turn.",
                damage: 5,
                range: 3,
                aoe: 0,
                turnDuration: 1,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(cavalier.className, cavalier);

        //Archer
        CharacterClass archer = new(
            name: "Archer",
            description: "Evasive and deals ranged damage",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 8,
                moveSpeed: 1,
                initiative: 3,
                range: 3,
                damageIterations: 1),
            ability: new(
                name: "Backflip + Root",
                description: "Moves 3 tiles away from current position and roots the nearest enemy until next turn.",
                damage: 0,
                range: 3,
                aoe: 0,
                turnDuration: 1,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(archer.className, archer);

        //Rogue
        CharacterClass rogue = new(
            name: "Rogue",
            description: "Can guarantee a treasure or a kill on low armor characters",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 7,
                moveSpeed: 2,
                initiative: 4,
                range: 1,
                damageIterations: 2),
            ability: new(
                name: "Stealth",
                description: "Can activate Stealth to become untargetable until he deals or is dealt damage.",
                damage: 0,
                range: 0,
                aoe: 0,
                turnDuration: 0,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(rogue.className, rogue);

        //Warrior
        CharacterClass warrior = new(
            name: "Warrior",
            description: "Tanky character with great mobility",
            stats: new(
                maxHealth: 15,
                armor: 4,
                damage: 5,
                moveSpeed: 2,
                initiative: 5,
                range: 1,
                damageIterations: 1),
            ability: new(
                name: "Charge",
                description: "Move towards an enemy and deal damage.",
                damage: 5,
                range: 3,
                aoe: 0,
                turnDuration: 0,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(warrior.className, warrior);

        //Paladin
        CharacterClass paladin = new(
            name: "Paladin",
            description: "Buffs allies and tanks",
            stats: new(
                maxHealth: 15,
                armor: 4,
                damage: 5,
                moveSpeed: 2,
                initiative: 6,
                range: 1,
                damageIterations: 1),
            ability: new(
                name: "Blessing",
                description: "Grants +2 to damage, health, armor and movement to all allies.",
                damage: 0,
                range: 0,
                aoe: 0,
                turnDuration: 2,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(paladin.className, paladin);

        //Druid
        CharacterClass druid = new(
            name: "Druid",
            description: "Impedes character movement, delaying damage or access to treasure",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 5,
                damageType: DamageType.magic,
                moveSpeed: 1,
                initiative: 7,
                range: 2,
                damageIterations: 1),
            ability: new(
                name: "Vine grasp",
                description: "Targets a tile and roots all enemies in a 2 tile radius.",
                damage: 0,
                range: 0,
                aoe: 2,
                turnDuration: 1,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(druid.className, druid);

        //Necromancer
        CharacterClass necromancer = new(
            name: "Necromancer",
            description: "Enables constant damage on distant targets but deals low damage by himself",
            stats: new(
                maxHealth: 8,
                armor: 3,
                damage: 3,
                damageType: DamageType.magic,
                moveSpeed: 1,
                initiative: 8,
                range: 999,
                damageIterations: 1),
            ability: new(
                name: "Soul Projection",
                description: "Targets any enemy and brings an effigy within two tiles that transfers received damage to targeted character until the next turn.",
                damage: 0,
                range: 2,
                aoe: 0,
                turnDuration: 1,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(necromancer.className, necromancer);

        //Wizard
        CharacterClass wizard = new(
            name: "Wizard",
            description: "Deals large ranged damage that ignores armor",
            stats: new(
                maxHealth: 8,
                armor: 1,
                damage: 5,
                damageType: DamageType.magic,
                moveSpeed: 1,
                initiative: 9,
                range: 3,
                damageIterations: 1),
            ability: new(
                name: "Fireball",
                description: "Throws a fireball at target character that explodes on contact, dealing damage adjacent tiles.",
                damage: 5,
                damageType: DamageType.magic,
                range: 3,
                aoe: 1,
                turnDuration: 0,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(wizard.className, wizard);

        //Priest
        CharacterClass priest = new(
            name: "Priest",
            description: "Healer",
            stats: new(
                maxHealth: 9,
                armor: 1,
                damage: 5,
                damageType: DamageType.healing,
                moveSpeed: 2,
                initiative: 10,
                range: 3,
                damageIterations: 1),
            ability: new(
                name: "Resurrect",
                description: "Revives an ally at 50% of max HP.",
                damage: 0,
                range: 999,
                aoe: 0,
                turnDuration: 0,
                use: (PlayerCharacter pc, Hex target) =>
                {
                    Debug.Log("Need to implement");
                })
            );
        GameController.AllClasses.Add(priest.className, priest);
    }
    #endregion
}