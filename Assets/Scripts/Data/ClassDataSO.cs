using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ClassData", order = 1)]
public class ClassDataSO : ScriptableObject
{
    [SerializeField]
    private List<GameObject> characterPrefabs;

    //keys are classID
    private Dictionary<int, CharacterClass> characterClasses;

    //singleton loaded from resources
    private const string resourcePath = "ClassData";
    private static ClassDataSO singleton = null;
    public static ClassDataSO Singleton {
        get {
            if (ClassDataSO.singleton == null)
                ClassDataSO.singleton = Resources.Load<ClassDataSO>(resourcePath);
            return ClassDataSO.singleton;
        }
    }

    private void Awake()
    {
        ClassDataSO.Singleton.characterClasses = this.DefineClasses().ToDictionary(charClass => charClass.classID);
    }

    public Sprite GetSpriteByClassID(int classID)
    {
        foreach (GameObject prefab in characterPrefabs)
        {
            if(prefab.GetComponent<PlayerCharacter>().charClassID == classID)
            {
                return prefab.GetComponent<SpriteRenderer>().sprite;
            }
        }

        throw new Exception("Requested sprite for undocumented classID.");
    }

    public GameObject GetPrefabByClassID(int classID)
    {
        foreach (GameObject prefab in characterPrefabs)
        {
            if (prefab.GetComponent<PlayerCharacter>().charClassID == classID)
            {
                return prefab;
            }
        }

        throw new Exception("Requested prefab for undocumented classID.");
    }

    public List<int> GetClassIDs()
    {
        return this.characterClasses.Keys.ToList<int>();
    }

    public CharacterClass GetClassByID(int classID)
    {
        return this.characterClasses[classID];
    }


    #region Static definitions

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    public List<CharacterClass> DefineClassesFromCSV(string statsFilename, string abilitiesFilname, string classFilename)
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

        return null;
    }

    public List<CharacterClass> DefineClasses()
    {
        List<CharacterClass> classes = new();

        CharacterStats barbStats = new(maxHealth: 10,
                                       armor: 2,
                                       damage: 5,
                                       damageType: DamageType.normal,
                                       moveSpeed: 3,
                                       initiative: 1,
                                       range: 1,
                                       damageIterations: 3);
        //Barb
        CharacterClass barbarian = new(
            classID: 0,
            name: "Barbarian",
            description: "Glass cannon, kill or get killed",
            stats: barbStats
        );
        classes.Add(barbarian);

        //Cavalier
        CharacterClass cavalier = new(
            classID: 1,
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
            abilities: new List<CharacterAbility> {
                new (
                    stringID: "CavalierStun",
                    interfaceName: "Lance Throw",
                    description: "Throws a lance at an enemy in a 3 tile radius, dealing damage and stunning the target until next turn.",
                    damage: 5,
                    range: 3,
                    aoe: 0,
                    turnDuration: 1,
                    actionType:typeof(CavalierImpaleAbility)
                )
            }
        );
        classes.Add(cavalier);

        //Archer
        CharacterClass archer = new(
            classID: 2,
            name: "Archer",
            description: "Evasive and deals ranged damage",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 8,
                moveSpeed: 1,
                initiative: 3,
                range: 3,
                damageIterations: 1)
            );
        classes.Add(archer);

        //Rogue
        CharacterClass rogue = new(
            classID: 3,
            name: "Rogue",
            description: "Can guarantee a treasure or a kill on low armor characters",
            stats: new(
                maxHealth: 10,
                armor: 2,
                damage: 7,
                moveSpeed: 2,
                initiative: 4,
                range: 1,
                damageIterations: 2)
            );
        classes.Add(rogue);

        //Warrior
        CharacterClass warrior = new(
            classID: 4,
            name: "Warrior",
            description: "Tanky character with great mobility",
            stats: new(
                maxHealth: 15,
                armor: 4,
                damage: 5,
                moveSpeed: 2,
                initiative: 5,
                range: 1,
                damageIterations: 1)
            );
        classes.Add(warrior);

        //Paladin
        CharacterClass paladin = new(
            classID: 5,
            name: "Paladin",
            description: "Buffs allies and tanks",
            stats: new(
                maxHealth: 15,
                armor: 4,
                damage: 5,
                moveSpeed: 2,
                initiative: 6,
                range: 1,
                damageIterations: 1)
            );
        classes.Add(paladin);

        //Druid
        CharacterClass druid = new(
            classID: 6,
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
                damageIterations: 1)
            );
        classes.Add(druid);

        //Necromancer
        CharacterClass necromancer = new(
            classID: 7,
            name: "Necromancer",
            description: "Enables constant damage on distant targets but deals low damage by himself",
            stats: new(
                maxHealth: 8,
                armor: 3,
                damage: 3,
                damageType: DamageType.magic,
                moveSpeed: 1,
                initiative: 8,
                range: 30,
                damageIterations: 1)
            );
        classes.Add(necromancer);

        //Wizard
        CharacterClass wizard = new(
            classID: 8,
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
                damageIterations: 1)
            );
        classes.Add(wizard);

        //Priest
        CharacterClass priest = new(
            classID: 9,
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
                damageIterations: 1,
                allowedAttackTargets: new List<TargetType> { TargetType.other_friendly_chars, TargetType.self })
            );
        classes.Add(priest);

        return classes;
    }
    #endregion
}
