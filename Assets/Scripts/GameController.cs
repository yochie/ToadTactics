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
    public Dictionary<string, CharacterClass> AllClasses { get; set; }
    public List<PlayerCharacter> PlayerChars = new List<PlayerCharacter>();

    public const int charsPerPlayer = 3;
    public Image[] CharacterSlotsUI = new Image[charsPerPlayer];

    public Map map;

    private void Awake()
    {
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.map.Initialize();
        this.InitClasses();

        //Create char test
        //PlayerCharacter testPlayer;
        //PlayerChars = new List<PlayerCharacter>();
        //for (int i = 0; i < AllPlayerCharPrefabs.Length; i++)
        //{
        //    PlayerChars.Add(Instantiate(this.AllPlayerCharPrefabs[i], new Vector3(0, 0, -0.1f), Quaternion.identity, map.GetHex(0, 0).transform));
        //    testPlayer = PlayerChars[i];
        //    testPlayer.Initialize(this.AllClasses.GetValueOrDefault("barbarian"), 0);
        //    Hex startingHexPos = this.map.GetHex(i - 6, 0);
        //    this.map.PlacePlayerChar(testPlayer, startingHexPos);
        //    //test ability
        //    testPlayer.CharClass.CharAbility.use(null, null);
        //}
    }

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private void InitClasses()
    {
        AllClasses = new Dictionary<string, CharacterClass>();

        //BARB
        CharacterClass barbarian = new CharacterClass();
        barbarian.Description = "He strong.";
        barbarian.CharStats = new(
            maxHealth: 10,
            armor: 2,
            damage: 5,
            speed: 3,
            initiative: 1,
            range: 1,
            damageIterations: 3);
        barbarian.CharAbility = new(
            name: "NA",
            description: "No active ability. Always attacks thrice.",
            damage: 1,
            range: 0,
            turnDuration: 0,
            use: (PlayerCharacter pc, Hex target) => {
                Debug.Log("Barb has no usable ability. Should probably prevent this from being called.");
            });
        AllClasses.Add("barbarian", barbarian);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
