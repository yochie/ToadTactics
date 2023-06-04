using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GameController : MonoBehaviour
{
    public PlayerCharacter[] playerCharPrefab = new PlayerCharacter[10];
    public List<PlayerCharacter> PlayerChars { get; set; }
    public Dictionary<string, CharacterClass> Classes { get; set; }

    public Map map;

    // Start is called before the first frame update
    void Start()
    {
        this.map.Initialize();
        this.InitClasses();

        //Create char
        PlayerCharacter testPlayer;
        PlayerChars = new List<PlayerCharacter>();
        for (int i = 0; i < playerCharPrefab.Length; i++) {
            PlayerChars.Add(Instantiate(this.playerCharPrefab[i], new Vector3(0,0,-0.1f), Quaternion.identity, map.GetHex(0,0).transform));
            testPlayer = PlayerChars[i];
            testPlayer.Initialize(this.Classes.GetValueOrDefault("barbarian"), 0);
            Hex startingHexPos = this.map.GetHex(i- 6, 0);
            this.map.PlacePlayerCharacter(testPlayer, startingHexPos);
            //test ability
            testPlayer.CharClass.CharAbility.use(null, null);
        }


    }

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private void InitClasses()
    {
        Classes = new Dictionary<string, CharacterClass>();

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
        Classes.Add("barbarian", barbarian);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
