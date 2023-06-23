using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CharacterClass
{
    public readonly int classID;
    public readonly string className;
    public readonly string classDescription;
    public readonly CharacterStats charStats;
    public readonly CharacterAbility charAbility;

    public CharacterClass(int classID, string name, string description, CharacterStats stats, CharacterAbility ability)
    {
        this.classID = classID;
        this.className = name;
        this.classDescription = description;
        this.charStats = stats;
        this.charAbility = ability;
    }

    #region Static definitions

    //Instantiate all classes to set their definitions here
    //TODO: find better spot to do this
    //probably should load from file (CSV)
    public static Dictionary<int, CharacterClass> DefineClassesFromCSV(string statsFilename, string abilitiesFilname, string classFilename)
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
    public static Dictionary<int, CharacterClass> DefineClasses()
    {
        Dictionary<int, CharacterClass> dictOfClasses = new();

        //Barb
        CharacterClass barbarian = new(
            classID: 0,
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
        dictOfClasses.Add(barbarian.classID, barbarian);

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
        dictOfClasses.Add(cavalier.classID, cavalier);

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
        dictOfClasses.Add(archer.classID, archer);

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
        dictOfClasses.Add(rogue.classID, rogue);

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
        dictOfClasses.Add(warrior.classID, warrior);

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
        dictOfClasses.Add(paladin.classID, paladin);

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
        dictOfClasses.Add(druid.classID, druid);

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
        dictOfClasses.Add(necromancer.classID, necromancer);

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
        dictOfClasses.Add(wizard.classID, wizard);

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
        dictOfClasses.Add(priest.classID, priest);

        return dictOfClasses;
    }
    #endregion
}
