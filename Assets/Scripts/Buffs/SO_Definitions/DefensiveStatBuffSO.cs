using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[CreateAssetMenu(fileName = "DefensiveBuff", menuName = "Buffs/DefensiveBuff")]
public class DefensiveStatBuffSO : ScriptableObject, IAppliablBuffDataSO, IHealthModifier, IArmorModifier, IMovementModifier
{
    //public string BuffTypeID => "PaladinTeamBuffData";
    //public string UIName => "Paladin team buff ";
    //public string IconName => "statbuff";
    //public bool IsPositive => true;
    //public bool NeedsToBeReAppliedEachTurn => false;
    //private const int HEALTH_OFFSET = 30;
    //private const int ARMOR_OFFSET = 5;
    //private const int MOVEMENT_OFFSET = 1;

    [field: SerializeField]
    public string stringID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField]
    public bool IsPositive { get; set; }

    [field: SerializeField]
    public DurationType DurationType { get; set; }

    [field: SerializeField]
    public int TurnDuration { get; set; }

    [field: SerializeField]
    public Sprite Icon { get; set; }

    [field: SerializeField]
    public int HealthOffset { get; set; }

    [field: SerializeField]
    public int ArmorOffset { get; set; }

    [field: SerializeField]
    public int MovementOffset { get; set; }

    [field: SerializeField]
    public bool NeedsToBeReAppliedEachTurn { get; set; }

    public void Apply(List<int> applyToCharacterIDs, bool isReapplication)
    {
        foreach(int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.ApplyStatModification(affectedCharacter);
        }
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.RemoveStatModification(affectedCharacter);
        }
    }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentMaxHealth = playerCharacter.CurrentStats.maxHealth;

        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, maxHealth: currentMaxHealth + this.HealthOffset));
        playerCharacter.TakeDamage(new Hit(HealthOffset, DamageType.healing));

        int currentArmor = playerCharacter.CurrentStats.armor;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, armor: currentArmor + this.ArmorOffset));

        int currentMoveSpeed = playerCharacter.CurrentStats.moveSpeed;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, moveSpeed: currentMoveSpeed + this.MovementOffset));
        playerCharacter.GrantMovesForTurn(this.MovementOffset);
    }

    public void RemoveStatModification(PlayerCharacter playerCharacter)
    {
        int previousMaxHealth = playerCharacter.CurrentStats.maxHealth;
        int previousHealth = playerCharacter.CurrentLife;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, maxHealth: previousMaxHealth - this.HealthOffset));
        playerCharacter.SetCurrentLife(Mathf.Clamp(playerCharacter.CurrentLife, 0, playerCharacter.CurrentStats.maxHealth));
        if (previousHealth > 0 && playerCharacter.CurrentLife <= 0)
        {
            playerCharacter.Die();
        }

        int currentArmor = playerCharacter.CurrentStats.armor;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, armor: currentArmor - this.ArmorOffset));

        int currentMoveSpeed = playerCharacter.CurrentStats.moveSpeed;
        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, moveSpeed: currentMoveSpeed - this.MovementOffset));
        playerCharacter.GrantMovesForTurn(-this.MovementOffset);
    }

    public string GetTooltipDescription()
    {
        return IBuffDataSO.GetDurationDescritpion(this);
    }

    public Dictionary<string, string> GetStatModificationsDictionnary()
    {
        Dictionary<string, string> toPrint = new();

        if(this.ArmorOffset != 0)
            toPrint.Add("Armor", System.String.Format("+{0}", this.ArmorOffset));
        if (this.HealthOffset != 0)
            toPrint.Add("Health", System.String.Format("+{0}", this.HealthOffset));
        if (this.MovementOffset != 0)
            toPrint.Add("Moves", System.String.Format("+{0}", this.MovementOffset));

        return toPrint;
    }
    public Dictionary<string, string> GetBuffStatsDictionary()
    {
        Dictionary<string, string> statsDictionary = new();
        this.GetStatModificationsDictionnary().ToList().ForEach(stat => { 
            if (statsDictionary.ContainsKey(stat.Key)) 
            { return; }
            statsDictionary.Add(stat.Key, stat.Value);
            });
        statsDictionary.Add("Duration", IBuffDataSO.GetDurationDescritpion(this));
        return statsDictionary;
    }
}
