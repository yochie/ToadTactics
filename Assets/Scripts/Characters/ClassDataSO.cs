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
    private List<PlayerCharacter> characterPrefabs;

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
        foreach (PlayerCharacter prefab in characterPrefabs)
        {
            if(prefab.charClassID == classID)
            {
                return prefab.GetComponent<SpriteRenderer>().sprite;
            }
        }

        throw new Exception("Requested sprite for undocumented classID.");
    }

    public PlayerCharacter GetPrefabByClassID(int classID)
    {
        foreach (PlayerCharacter prefab in characterPrefabs)
        {
            if (prefab.charClassID == classID)
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

    public int GetRandomClassID()
    {
        List<int> classIDs = ClassDataSO.Singleton.GetClassIDs();
        int numClassIDs = classIDs.Count;

        return classIDs[UnityEngine.Random.Range(0, numClassIDs)];
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

        CharacterStats barbStats = new(maxHealth: 170,
                                       armor: 5,
                                       damage: 20,
                                       critChance: 0.15f,
                                       critMultiplier: 1.3f,
                                       damageType: DamageType.physical,
                                       moveSpeed: 4,
                                       initiative: 1,
                                       range: 1,
                                       damageIterations: 2);
        //Barb
        CharacterClass barbarian = new(
            classID: 0,
            name: "Barbarian",
            description: "A barbarian is a primal warrior, embodying raw power and untamed fury on the battlefield. They eschew finesse and tactical subtlety in favor of sheer physical might. Wielding massive weapons in each hand, barbarians charge into combat with a relentless assault.",
            stats: barbStats
        );
        classes.Add(barbarian);

        //Cavalier
        CharacterClass cavalier = new(
            classID: 1,
            name: "Cavalier",
            description: "A cavalier is a knightly warrior, embodying the virtues of courage, honor, and nobility. They are skilled riders, mounted on majestic steeds, and known for their prowess in both mounted and on-foot combat. Cavaliers are defenders of justice and champions of their allies, carrying the banner of honor into battle.",
            stats: new(
                maxHealth: 180,
                armor: 8,
                damage: 30,
                critChance: 0.15f,
                critMultiplier: 1.5f,
                moveSpeed: 4,
                initiative: 2,
                range: 2,
                damageIterations: 1),
            abilities: new List<CharacterAbilityStats> {
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
            description: "An archer is a highly skilled marksman, specializing in ranged attacks and deadly accuracy. They are masters of long-range combat, utilizing bows, to deliver precise and devastating shots from a safe distance. Archers excel at dealing sustained damage and evade their enemies attacks with grace and agility.",
            stats: new(
                maxHealth: 150,
                armor: 5,
                damage: 30,
                critChance: 0.2f,
                critMultiplier: 1.6f,
                moveSpeed: 2,
                initiative: 3,
                range: 3,
                damageIterations: 1)
            );
        classes.Add(archer);

        //Rogue
        CharacterClass rogue = new(
            classID: 3,
            name: "Rogue",
            description: "A rogue is a nimble and deadly character, specializing in covert operations, precise strikes, and evasion tactics. They excel at quickly dispatching enemies with swift attacks and taking advantage of their opponents' vulnerabilities. Armed with dual daggers rogues are masters of close-quarters combat.",
            stats: new(
                maxHealth: 170,
                armor: 6,
                damage: 15,
                critChance: 0.3f,
                critMultiplier: 1.8f,
                moveSpeed: 3,
                initiative: 4,
                range: 1,
                damageIterations: 2)
            );
        classes.Add(rogue);

        //Warrior
        CharacterClass warrior = new(
            classID: 4,
            name: "Warrior",
            description: "A warrior is a seasoned and battle-hardened fighter, wielding weapons and armor with exceptional skill and precision. They are masters of martial combat, specializing in close-quarters combat and being at the forefront of the battlefield. Warriors possess a deep understanding of various weapons and can adapt their fighting style to suit different situations.",
            stats: new(
                maxHealth: 180,
                armor: 8,
                damage: 30,
                critChance: 0.2f,
                critMultiplier: 1.5f,
                moveSpeed: 3,
                initiative: 5,
                range: 1,
                damageIterations: 1)
            );
        classes.Add(warrior);

        //Paladin
        CharacterClass paladin = new(
            classID: 5,
            name: "Paladin",
            description: "A paladin is a righteous champion, wielding both martial skills and holy magic in service of a higher purpose. They are imbued with divine favor and possess a deep connection to the forces of light. Paladins combine the might of a warrior with the protective abilities of a holy caster, making them versatile and valuable assets in any group.",
            stats: new(
                maxHealth: 200,
                armor: 10,
                damage: 20,
                critChance: 0.1f,
                critMultiplier: 1.5f,
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
            description: "A druid is a guardian of nature, deeply attuned to the natural balance of the world. They draw their power from the spirits of the wild, enabling them to wield primal magic. Druids are also capable of tapping into the forces of nature to support their allies and hinder their enemies. ",
            stats: new(
                maxHealth: 150,
                armor: 6,
                damage: 20,
                damageType: DamageType.magic,
                critChance: 0.1f,
                critMultiplier: 1.5f,
                moveSpeed: 2,
                initiative: 7,
                range: 3,
                damageIterations: 1)
            ); ;
        classes.Add(druid);

        //Necromancer
        CharacterClass necromancer = new(
            classID: 7,
            name: "Necromancer",
            description: "A grim figure living only to inflict excruciating pain on his foes. The necromancer feasts on souls and channels his spells through dark rituals. Defying the physical realm of mortals, their magic tears through the flesh of opponents.",
            stats: new(
                maxHealth: 160,
                armor: 3,
                damage: 20,
                damageType: DamageType.magic,
                attacksRequireLOS: false,
                critChance: 0.1f,
                critMultiplier: 1.5f,
                moveSpeed: 3,
                initiative: 8,
                range: 20,
                damageIterations: 1)
            );
        classes.Add(necromancer);

        //Wizard
        CharacterClass wizard = new(
            classID: 8,
            name: "Wizard",
            description: "A wizard is a scholar of arcane arts, delving into the mysteries of magic to harness its immense power. They are masters of the elemental forces and possess an extensive repertoire of spells that can shape reality itself. Wizards command devastating spells, annihilate their foes and outmaneuver them.",
            stats: new(
                maxHealth: 150,
                armor: 3,
                damage: 30,
                damageType: DamageType.magic,
                critChance: 0.2f,
                critMultiplier: 1.3f,
                moveSpeed: 2,
                initiative: 9,
                range: 3,
                damageIterations: 1)
            );
        classes.Add(wizard);

        //Priest
        CharacterClass priest = new(
            classID: 9,
            name: "Priest",
            description: "A priest is a devout and dedicated individual, devoted to the divine arts and the well-being of their allies. They draw their power from their unwavering faith and their connection to higher beings. As masters of the light, they keep their allies healthy and save them from certain death.",
            stats: new(
                maxHealth: 150,
                armor: 3,
                damage: 30,
                damageType: DamageType.healing,
                attacksRequireLOS: false,
                critChance: 0.1f,
                critMultiplier: 1.5f,
                moveSpeed: 3,
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