using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using Mirror;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton { get; private set; }
    public GameObject[] AllPlayerCharPrefabs = new GameObject[10];
    public static Dictionary<string, CharacterClass> AllClasses { get; set; }
    public PlayerController LocalPlayer { get; set; }

    //Todo: spawn at runtime to allow gaining new slots for clone or losing slots for amalgam
    public List<CharacterSlotUI> characterSlotsUI = new();

    //maps prefab indexes to player indexes
    public readonly SyncDictionary<int, int> characterOwners = new();

    public GameObject turnOrderSlotPrefab;
    
    public GameObject turnOrderBar;
    
    public List<TurnOrderSlotUI> turnOrderSlots = new();

    //TODO : make this a netid syncronized var like map hexgrid
    //should map prefabID to netid
    public readonly SyncDictionary<int, uint> AllPlayerCharactersIDs = new();
    public readonly Dictionary<int, PlayerCharacter> playerCharacters = new();

    //maps character initiative to prefab indexes
    public readonly SyncIDictionary<float, int> turnOrderSortedPrefabIds = new SyncIDictionary<float,int>(new SortedList<float,int>());

    //stores index of turn in turnOrderSortedPrefabIds
    //NOT the initiative used as keys, rather the index in the ordered list
    [SyncVar(hook = nameof(OnCharacterTurnChanged))]
    public int characterTurnOrderIndex = 0;

    //stores currently playing player's ID
    [SyncVar(hook = nameof(OnPlayerTurnChanged))]
    public int playerTurn = 0;

    [SyncVar]
    public GameMode currentGameMode = GameMode.gameplay;

    public GameObject endTurnButton;

    public Map map;

    private void Awake()
    {
        Singleton = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.AllPlayerCharactersIDs.Callback += OnAllPlayerCharacterIdsChange;
        // Process initial SyncDictionary payload
        foreach (KeyValuePair<int, uint> kvp in AllPlayerCharactersIDs)
            OnAllPlayerCharacterIdsChange(SyncDictionary<int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);

        turnOrderSortedPrefabIds.Callback += OnTurnOrderChanged;
        foreach (KeyValuePair<float, int> kvp in turnOrderSortedPrefabIds)
            OnTurnOrderChanged(SyncDictionary<float, int>.Operation.OP_ADD, kvp.Key, kvp.Value);

        GameController.DefineClasses();

        this.map.Initialize();
    }

    [Client]
    private void OnAllPlayerCharacterIdsChange(SyncIDictionary<int, uint>.Operation op, int key, uint netidArg)
    {
        switch (op)
        {
            case SyncDictionary<int, uint>.Operation.OP_ADD:
                // entry added
                Debug.Log("Callback for character being added to Gamecontroller main list has been called.");
                Debug.LogFormat("Adding character with key {0}", key);
                this.playerCharacters[key] = null;

                if (NetworkClient.spawned.TryGetValue(netidArg, out NetworkIdentity netidObject))
                {
                    Debug.LogFormat("Actually adding character with key {0}", key);
                    this.playerCharacters[key] = netidObject.gameObject.GetComponent<PlayerCharacter>();
                }
                else
                {
                    Debug.LogFormat("Couldnt find character with key {0}, calling coroutine", key);

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

    [Client]
    private IEnumerator PlayerFromNetIDCoroutine(int key, uint netIdArg)
    {
        while (this.playerCharacters[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
            {
                Debug.LogFormat("Actually adding character with key {0} from coroutine", key);

                this.playerCharacters[key] = identity.gameObject.GetComponent<PlayerCharacter>();
            }
        }
    }

    public void Start()
    {
        if (!IsItMyClientsTurn())
            this.endTurnButton.SetActive(false);

        //set initial turn UI for character turn 0
        OnCharacterTurnChanged(-1, 0);

        ResetCharacterTurn();
    }

    //turnOrderSortedPrefabIds callback
    private void OnTurnOrderChanged(SyncIDictionary<float, int>.Operation op, float key, int value)
    {
        switch (op)
        {
            case SyncIDictionary<float, int>.Operation.OP_ADD:
                // entry added
                Debug.LogFormat("Adding {0} with priority {1}", AllPlayerCharPrefabs[value].name, key);
                GameObject slot = Instantiate(this.turnOrderSlotPrefab, this.turnOrderBar.transform);
                turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());
                this.UpdateTurnOrderSlotsUI();
                break;

            case SyncIDictionary<float, int>.Operation.OP_SET:
                // entry changed
                break;

            case SyncIDictionary<float, int>.Operation.OP_REMOVE:
                // entry removed
                Debug.LogFormat("Removing {0} with priority {1}", AllPlayerCharPrefabs[value].name, key);
                foreach(TurnOrderSlotUI currentSlot in turnOrderSlots)
                {
                    if(currentSlot.holdsPrefabWithIndex == value)
                    {
                        Debug.LogFormat("Destroying slot with {0}", AllPlayerCharPrefabs[value].name);
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

    //called by commands to modify turnOrderSortedPrefabIds
    [Server]
    internal void AddMyChar(int playerIndex, int prefabIndex, int initiative)
    {
        this.characterOwners.Add(prefabIndex, playerIndex);

        //throws callback to update UI
        this.turnOrderSortedPrefabIds.Add(initiative, prefabIndex);
    }

    //called by commands to modify turnOrderSortedPrefabIds
    [Server]
    internal void RemoveAllMyChars(int playerIndex)
    {
        List<float> ownedToRemove = new();
        foreach (int characterPrefabId in this.characterOwners.Keys)
        {
            if(this.characterOwners[characterPrefabId] == playerIndex)
            {
                List<float> turnToRemove = new();
                foreach (float initiative in this.turnOrderSortedPrefabIds.Keys)
                {
                    if (this.turnOrderSortedPrefabIds[initiative] == characterPrefabId)
                    {
                        turnToRemove.Add(initiative);
                    }
                }
                foreach (float toRemove in turnToRemove)
                {
                    this.turnOrderSortedPrefabIds.Remove(toRemove);
                }

                ownedToRemove.Add(characterPrefabId);                
            }
        }

        foreach(int toRemove in ownedToRemove)
        {
            this.characterOwners.Remove(toRemove);
        }
    }

    //used to update UI from callback
    private void UpdateTurnOrderSlotsUI()
    {
        Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach(float initiative in this.turnOrderSortedPrefabIds.Keys)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            if (i >= this.turnOrderSlots.Count) return;

            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            Image slotImage = slot.GetComponent<Image>();
            int prefabId = this.turnOrderSortedPrefabIds[initiative];
            slotImage.sprite = AllPlayerCharPrefabs[prefabId].GetComponent<SpriteRenderer>().sprite;
            slot.holdsPrefabWithIndex = prefabId;
            
            if (this.characterTurnOrderIndex == i)
            {
                slot.HighlightAndLabel(i+1);
            } else
            {
                slot.UnhighlightAndLabel(i+1);
            }
            i++;
        }
    }

    //modifies syncvars currentTurnPlayer and characterTurnOrderIndex
    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        if(this.currentGameMode == GameMode.draft ||
            this.currentGameMode == GameMode.characterPlacement ||
            this.currentGameMode == GameMode.treasureDraft ||
            this.currentGameMode == GameMode.treasureEquip)
        {
            this.SwapPlayerTurn();
        } else if(this.currentGameMode == GameMode.gameplay)
        {
            this.NextCharacterTurn();
        }
    }

    //callback for characterTurnOrderIndex
    private void OnCharacterTurnChanged(int prevTurnIndex, int newTurnIndex)
    {
        Debug.Log("OnCharacterTurnChanged");

        //finds prefab ID for character whose turn it is
        int newTurnCharacterId = this.PrefabIdForPlayingCharacter();

        //highlights turnOrderSlotUI for currentyl playing character
        int i = 0;
        foreach(TurnOrderSlotUI slot in turnOrderSlots)
        {
            i++;
            if (slot.holdsPrefabWithIndex == newTurnCharacterId)
            {
                Debug.Log("Highlighting slot");
                slot.HighlightAndLabel(i);
            } else
            {
                slot.UnhighlightAndLabel(i);

            }
        }
    }

    //callback for currentTurnPlayer
    private void OnPlayerTurnChanged(int _, int newPlayer)
    {
        if (newPlayer == this.LocalPlayer.playerIndex)
        {
            //display "Its your turn" msg

            Debug.Log("Its your turn");
            this.endTurnButton.SetActive(true);
        }
        else
        {
            //display "Waiting for other player" msg
            Debug.Log("Waiting for other player to end their turn.");
            this.endTurnButton.SetActive(false);

        }
    }

    [Server]
    private void NextCharacterTurn()
    {
        //loops through turn order        
        this.characterTurnOrderIndex++;
        if (this.characterTurnOrderIndex >= this.turnOrderSortedPrefabIds.Count)
            this.characterTurnOrderIndex = 0;

        //finds character prefab id for the next turn so that we can check who owns it
        int currentCharacterPrefab = -1;
        int i = 0;
        foreach (int prefab in this.turnOrderSortedPrefabIds.Values)
        {
            if (i == this.characterTurnOrderIndex)
            {
                currentCharacterPrefab = prefab;
            }
            i++;
        }
        if(currentCharacterPrefab == -1)
        {
            Debug.Log("Error : Bad code for iterating turnOrderSortedPrefabIds");
        }

        //if we don't own that char, swap player turn
        if(this.playerTurn != characterOwners[currentCharacterPrefab])
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
    public void ResetCharacterTurn()
    {
        Debug.Log("Resetting turn data");
        this.characterTurnOrderIndex = 0;
        this.playerTurn = 0;

        //finds character prefab id for the next turn so that we can check who owns it
        int currentCharacterPrefab = -1;
        int i = 0;
        foreach (int prefab in this.turnOrderSortedPrefabIds.Values)
        {
            if (i == this.characterTurnOrderIndex)
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
        if (this.characterOwners[prefabID] == this.LocalPlayer.playerIndex) {
            return true;
        } else
        {
            return false;
        }
    }

    public bool DoesHeOwnThisCharacter(int playerIndex, int prefabID)
    {
        if (this.characterOwners[prefabID] == playerIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int PrefabIdForPlayingCharacter(int playingCharacterIndex = -1)
    {
        //finds prefab ID for character whose turn it is
        int currentyPlaying = (playingCharacterIndex == -1 ? this.characterTurnOrderIndex : playingCharacterIndex);
        int i = 0;
        foreach (int prefabId in this.turnOrderSortedPrefabIds.Values)
        {
            if (i == currentyPlaying)
            {
                return prefabId;
            } else
            {
                i++;
            }
            
        }

        return -1;
    }


    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private static void DefineClasses()
    {
        GameController.AllClasses = new Dictionary<string, CharacterClass>();

        //Barb
        CharacterClass barbarian = new();
        barbarian.Description = "Glass cannon, kill or get killed";
        barbarian.CharStats = new(
            maxHealth: 10,
            armor: 2,
            damage: 5,
            damageType: DamageType.normal,
            moveSpeed: 3,
            initiative: 1,
            range: 1,
            damageIterations: 3);
        barbarian.CharAbility = new(
            name: "NA",
            description: "No active ability. Always attacks thrice.",
            damage: 1,
            range: 0,
            aoe: 0,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Barb has no usable ability. Should probably prevent this from being called.");
            });
        GameController.AllClasses.Add("Barbarian", barbarian);

        //Cavalier
        CharacterClass cavalier = new();
        cavalier.Description = "Rapidly reaches the backline or treasure";
        cavalier.CharStats = new(
            maxHealth: 12,
            armor: 3,
            damage: 5,
            moveSpeed: 3,
            initiative: 2,
            range: 2,
            damageIterations: 1);
        cavalier.CharAbility = new(
            name: "Lance Throw",
            description: "Throws a lance at an enemy in a 3 tile radius, dealing damage and stunning the target until next turn.",
            damage: 5,
            range: 3,
            aoe: 0,
            turnDuration: 1,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Cavalier", cavalier);

        //Archer
        CharacterClass archer = new();
        archer.Description = "Evasive and deals ranged damage";
        archer.CharStats = new(
            maxHealth: 10,
            armor: 2,
            damage: 8,
            moveSpeed: 1,
            initiative: 3,
            range: 3,
            damageIterations: 1);
        archer.CharAbility = new(
            name: "Backflip + Root",
            description: "Moves 3 tiles away from current position and roots the nearest enemy until next turn.",
            damage: 0,
            range: 3,
            aoe:0,
            turnDuration: 1,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Archer", archer);

        //Rogue
        CharacterClass rogue = new();
        rogue.Description = "Can guarantee a treasure or a kill on low armor characters";
        rogue.CharStats = new(
            maxHealth: 10,
            armor: 2,
            damage: 7,
            moveSpeed: 2,
            initiative: 4,
            range: 1,
            damageIterations: 2);
        rogue.CharAbility = new(
            name: "Stealth",
            description: "Can activate Stealth to become untargetable until he deals or is dealt damage.",
            damage: 0,
            range: 0,
            aoe: 0,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Rogue", rogue);

        //Warrior
        CharacterClass warrior = new();
        warrior.Description = "Tanky character with great mobility";
        warrior.CharStats = new(
            maxHealth: 15,
            armor: 4,
            damage: 5,
            moveSpeed: 2,
            initiative: 5,
            range: 1,
            damageIterations: 1);
        warrior.CharAbility = new(
            name: "Charge",
            description: "Move towards an enemy and deal damage.",
            damage: 5,
            range: 3,
            aoe: 0,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Warrior", warrior);

        //Paladin
        CharacterClass paladin = new();
        paladin.Description = "Buffs allies and tanks";
        paladin.CharStats = new(
            maxHealth: 15,
            armor: 4,
            damage: 5,
            moveSpeed: 2,
            initiative: 6,
            range: 1,
            damageIterations: 1);
        paladin.CharAbility = new(
            name: "Blessing",
            description: "Grants +2 to damage, health, armor and movement to all allies.",
            damage: 0,
            range: 0,
            aoe: 0,
            turnDuration: 2,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Paladin", paladin);

        //Druid
        CharacterClass druid = new();
        druid.Description = "Impedes character movement, delaying damage or access to treasure";
        druid.CharStats = new(
            maxHealth: 10,
            armor: 2,
            damage: 5,
            damageType: DamageType.magic,
            moveSpeed: 1,
            initiative: 7,
            range: 2,
            damageIterations: 1);
        druid.CharAbility = new(
            name: "Vine grasp",
            description: "Targets a tile and roots all enemies in a 2 tile radius.",
            damage: 0,
            range: 0,
            aoe: 2,
            turnDuration: 1,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Druid", druid);

        //Necromancer
        CharacterClass necromancer = new();
        necromancer.Description = "Enables constant damage on distant targets but deals low damage by himself";
        necromancer.CharStats = new(
            maxHealth: 8,
            armor: 3,
            damage: 3,
            damageType: DamageType.magic,
            moveSpeed: 1,
            initiative: 8,
            range: 999,
            damageIterations: 1);
        necromancer.CharAbility = new(
            name: "Soul Projection",
            description: "Targets any enemy and brings an effigy within two tiles that transfers received damage to targeted character until the next turn.",
            damage: 0,
            range: 2,
            aoe: 0,
            turnDuration: 1,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Necromancer", necromancer);

        //Wizard
        CharacterClass wizard = new();
        wizard.Description = "Deals large ranged damage that ignores armor";
        wizard.CharStats = new(
            maxHealth: 8,
            armor: 1,
            damage: 5,
            damageType: DamageType.magic,
            moveSpeed: 1,
            initiative: 9,
            range: 3,
            damageIterations: 1);
        wizard.CharAbility = new(
            name: "Fireball",
            description: "Throws a fireball at target character that explodes on contact, dealing damage adjacent tiles.",
            damage: 5,
            damageType: DamageType.magic,
            range: 3,
            aoe: 1,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Wizard", wizard);

        //Priest
        CharacterClass priest = new();
        priest.Description = "Healer";
        priest.CharStats = new(
            maxHealth: 9,
            armor: 1,
            damage: 5,
            damageType: DamageType.healing,
            moveSpeed: 2,
            initiative: 10,
            range: 3,
            damageIterations: 1);
        priest.CharAbility = new(
            name: "Resurrect",
            description: "Revives an ally at 50% of max HP.",
            damage: 0,
            range: 999,
            aoe: 0,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Need to implement");
            });
        GameController.AllClasses.Add("Priest", priest);
    }
}