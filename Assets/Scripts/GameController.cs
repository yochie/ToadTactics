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
    public PlayerController LocalPlayer { get; set; }

    public List<PlayerCharacter> PlayerChars = new();

    public const int charsPerPlayer = 3;
    public CharacterSlotUI[] CharacterSlotsUI = new CharacterSlotUI[charsPerPlayer];

    public Map map;

    private void Awake()
    {
        Singleton = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.map.Initialize();
        this.InitClasses();
    }

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private void InitClasses()
    {
        AllClasses = new Dictionary<string, CharacterClass>();

        //BARB
        CharacterClass barbarian = new();
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
}
