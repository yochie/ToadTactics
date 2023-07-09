using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CharacterClass
{
    public readonly int classID;
    public readonly string name;
    public readonly string description;
    public readonly CharacterStats stats;
    public readonly List<CharacterAbility> abilities = new();


    public CharacterClass(int classID, string name, string description, CharacterStats stats, List<CharacterAbility> abilities = null)
    {
        this.classID = classID;
        this.name = name;
        this.description = description;
        this.stats = stats;
        this.abilities = abilities;
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
                damageIterations: 3)
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
            abilities: new List<CharacterAbility> { 
                new(
                    name: "Lance Throw",
                    description: "Throws a lance at an enemy in a 3 tile radius, dealing damage and stunning the target until next turn.",
                    damage: 5,
                    range: 3,
                    aoe: 0,
                    turnDuration: 1,
                    abilityActionType: typeof(CavalierImpaleAbility))
            }
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
                damageIterations: 1)
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
                damageIterations: 2)
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
                damageIterations: 1)
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
                damageIterations: 1)
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
                damageIterations: 1)
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
                damageIterations: 1)
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
                damageIterations: 1)
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
                damageIterations: 1,
                allowedAttackTargets: new List<TargetType> { TargetType.other_friendly_chars, TargetType.self })
            );
        dictOfClasses.Add(priest.classID, priest);

        return dictOfClasses;
    }
    #endregion
}
