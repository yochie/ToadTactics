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
    private Dictionary<string, Type> abilityActionTypes;
    private Dictionary<string, Type> passiveAbilitiesBuffTypes;

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
        ClassDataSO.Singleton.abilityActionTypes = this.LinkAbilitiesToTheirActionTypes();
        ClassDataSO.Singleton.passiveAbilitiesBuffTypes = this.LinkPassiveAbilitiesToTheirBuffTypes();

    }

    public Sprite GetSpriteByClassID(int classID)
    {
        foreach (PlayerCharacter prefab in characterPrefabs)
        {
            if(prefab.CharClassID == classID)
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
            if (prefab.CharClassID == classID)
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

    public Type GetActionTypeByID(string abilityID)
    {        
        return this.abilityActionTypes[abilityID];
    }

    public Type GetBuffTypesByPassiveAbilityID(string abilityID)
    {
        return this.passiveAbilitiesBuffTypes[abilityID];
    }



    #region Static definitions

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    private List<CharacterClass> DefineClassesFromCSV(string statsFilename, string abilitiesFilname, string classFilename)
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

    private List<CharacterClass> DefineClasses()
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
                                       damageIterations: 2,
                                       hasFaith: false);
        //Barb
        CharacterClass barbarian = new(
            classID: 0,
            name: "Barbarian",
            description: "A barbarian is a primal warrior, embodying raw power and untamed fury on the battlefield. They eschew finesse and tactical subtlety in favor of sheer physical might. Wielding massive weapons in each hand, barbarians charge into combat with a relentless assault.",
            stats: barbStats,
            abilities: new List<CharacterAbilityStats> { 
                new (
                    stringID: "BarbarianKingDamage",
                    interfaceName: "KingSlayer",
                    description: "Grants a bonus to damage when attacking the king.",
                    isPassive: true
                )
            }
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
                damageIterations: 1,
                hasFaith: false),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "CavalierStun",
                    interfaceName: "Lance Throw",
                    description: "Throws a lance at an enemy in a 3 tile radius, dealing damage and stunning the target until next turn.",
                    damage: 20,
                    damageIterations: 1,
                    damageType: DamageType.physical,
                    range: 3,
                    buffTurnDuration: 1,
                    allowedAbilityTargets: new List<TargetType>(){TargetType.ennemy_chars, TargetType.obstacle },
                    cooldownDuration: 3,
                    cappedByCooldown: true
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
                damageIterations: 1,
                hasFaith: false),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "ArcherSnipe",
                    interfaceName: "Snipe",
                    description: "Targets an enemy with a shot that has infinite range that pierces all targets and destroys trees between the target and the Archer. The attack does not require LOS.",
                    damage: 30,
                    damageIterations: 1,
                    damageType: DamageType.physical,
                    range: Utility.MAX_DISTANCE_ON_MAP,
                    requiresLOS: false,
                    allowedAbilityTargets: new List<TargetType>(){TargetType.ennemy_chars, TargetType.obstacle},
                    cooldownDuration: 3,
                    cappedByCooldown: true,
                    piercesLOS: true
                ) 
            }
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
                damageIterations: 2,
                hasFaith: false),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "RogueCrit",
                    interfaceName: "Fatal Strike",
                    description: "Targets an enemy and deals the Rogue's base damage (once) with a guaranteed critical strike that also ignore armor.",
                    damage: 15,
                    damageIterations: 1,
                    damageType: DamageType.physical,
                    range: 1,                  
                    allowedAbilityTargets: new List<TargetType>(){TargetType.ennemy_chars, TargetType.obstacle },
                    cooldownDuration: 3,
                    cappedByCooldown: true,
                    canCrit: true,
                    critChance: 1f,
                    critMultiplier: -1f,
                    penetratingDamage: true
                )
            }
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
                damageIterations: 1,
                hasFaith: false),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "WarriorRoot",
                    interfaceName: "Intimidating Shout",
                    description: "Shouts and scares close enemies, making them cower in fear for a turn.",
                    buffTurnDuration: 1,
                    allowedAbilityTargets: new List<TargetType>(){ TargetType.self },
                    aoe : 1,
                    range: 0,
                    cappedByCooldown: true,
                    cooldownDuration: 5
                )
            }
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
                damageIterations: 1,
                hasFaith: true),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "PaladinTeamBuff",
                    interfaceName: "Blessing of Kings",
                    description: "Grants a bonus to health, armor and movement to all allies.",
                    buffTurnDuration: 2,
                    allowedAbilityTargets: new List<TargetType>(){ TargetType.self },
                    cooldownDuration: 4, 
                    cappedByCooldown: true,
                    range:0
                )
            }
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
                damageIterations: 1,
                hasFaith: true),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "DruidLava",
                    interfaceName: "Fissure",
                    description: "Targets a tile and fissures the ground causing lava to erupt onto the battlefield.",
                    allowedAbilityTargets: Utility.GetAllEnumValues<TargetType>(),
                    cooldownDuration: 3,
                    cappedByCooldown: true,
                    range : 3,
                    aoe: 1,
                    requiresLOS: false,
                    damage: HazardDataSO.Singleton.GetHazardDamage(HazardType.fire),
                    damageIterations: 1,
                    damageType: HazardDataSO.Singleton.GetHazardDamageType(HazardType.fire)
                )
            }
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
                moveSpeed: 2,
                initiative: 8,
                range: 3,
                damageIterations: 1,
                hasFaith: true),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "NecroDOT",
                    interfaceName: "Rotting Corpse",
                    description: "Self harms in order to inflict a curse that deals stackable damage over time on an enemy.",
                    allowedAbilityTargets: new List<TargetType>(){ TargetType.ennemy_chars },
                    cooldownDuration: 1,
                    cappedByCooldown: true,
                    range: 3,
                    requiresLOS: true,
                    damage: 10, //this is for self damage, debuff damage defined in buff effect class
                    damageIterations: 1,
                    damageType: DamageType.magic
                )
            }
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
                damageIterations: 1,
                hasFaith: false),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "WizardFireball",
                    interfaceName: "Fireball",
                    description: "Target any tile and throw exploding fireball that deals magic damage in area of effect.",                    
                    allowedAbilityTargets: Utility.GetAllEnumValues<TargetType>(),
                    cooldownDuration: 2,
                    cappedByCooldown: true,
                    range: 3,
                    aoe: 1,
                    damage: 30,
                    damageIterations: 1,
                    damageType: DamageType.magic
                ) 
            }
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
                allowedAttackTargets: new List<TargetType> { TargetType.other_friendly_chars, TargetType.self },
                hasFaith: true),
            abilities: new List<CharacterAbilityStats> {
                new (
                    stringID: "PriestResurrect",
                    interfaceName: "Resurrect",
                    description: "Resurrect ally with half of maximum health.",                    
                    allowedAbilityTargets: new List<TargetType>(){ TargetType.friendly_corpse },
                    usesPerRound: 1,
                    cappedPerRound: true,
                    requiresLOS: false,
                    range: Utility.MAX_DISTANCE_ON_MAP
                )
            }
            );
        classes.Add(priest);

        return classes;
    }

    private Dictionary<string, Type> LinkAbilitiesToTheirActionTypes()
    {
        Dictionary<string, Type> actionsByAbilityID = new();

        actionsByAbilityID.Add("CavalierStun", typeof(CavalierStunAbility));
        actionsByAbilityID.Add("PaladinTeamBuff", typeof(PaladinTeamBuffAbility));
        actionsByAbilityID.Add("PriestResurrect", typeof(PriestResurrectAbility));
        actionsByAbilityID.Add("WizardFireball", typeof(WizardFireballAbility));      
        actionsByAbilityID.Add("WarriorRoot", typeof(WarriorRootAbility));
        actionsByAbilityID.Add("NecroDOT", typeof(NecroDOTAbility));
        actionsByAbilityID.Add("RogueCrit", typeof(RogueCritAbility));
        actionsByAbilityID.Add("ArcherSnipe", typeof(ArcherSnipeAbility));
        actionsByAbilityID.Add("DruidLava", typeof(DruidLavaAbility));

        return actionsByAbilityID;
    }

    private Dictionary<string, Type> LinkPassiveAbilitiesToTheirBuffTypes()
    {
        Dictionary<string, Type> buffsByPassiveAbilityID = new();

        buffsByPassiveAbilityID.Add("BarbarianKingDamage", typeof(BarbarianKingDamageEffect));
        return buffsByPassiveAbilityID;
    }
    #endregion
}
