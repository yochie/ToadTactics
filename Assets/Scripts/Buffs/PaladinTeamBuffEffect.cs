using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PaladinTeamBuffEffect : IAbilityBuffEffect, IHealthModifier, IArmorModifier, IMovementModifier, ITimedEffect
{
    #region IBuffEffect
    public string BuffTypeID => "PaladinTeamBuffEffect";
    public string UIName => "Paladin team buff ";
    public string IconName => "statbuff";
    public bool IsPositive => true;
    public bool NeedsToBeReAppliedEachTurn => false;
    // set at runtime
    public int UniqueID { get; set; }
    public List<int> AffectedCharacterIDs { get; set; }

    #endregion

    #region IStatModifier
    private const int HEALTH_OFFSET = 30;
    private const int ARMOR_OFFSET = 5;
    private const int MOVEMENT_OFFSET = 1;

    public int HealthOffset { get => HEALTH_OFFSET; set => throw new NotSupportedException(); }
    public int ArmorOffset { get => ARMOR_OFFSET; set => throw new NotSupportedException(); }
    public int MovementOffset { get => MOVEMENT_OFFSET; set => throw new NotSupportedException(); }
    #endregion

    #region IAbilityBuffEffect
    //set at runtime
    public int ApplyingCharacterID {get; set;}
    public CharacterAbilityStats AppliedByAbility { get; set; }
    #endregion

    #region ITimedEffect
    public int TurnDurationRemaining { get; set; }

    #endregion

    #region IBuffEffect functions
    public bool ApplyEffect(List<int> applyToCharacterIDs, bool isReapplication)
    {
        foreach(int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.ApplyStatModification(affectedCharacter);
        }
        return true;
    }

    public void UnApply(List<int> applyToCharacterIDs)
    {
        foreach (int affectedCharacterID in applyToCharacterIDs)
        {
            PlayerCharacter affectedCharacter = GameController.Singleton.PlayerCharactersByID[affectedCharacterID];
            this.RemoveStatModification(affectedCharacter);
        }
    }
    #endregion

    #region IStatModifier functions
    public Dictionary<string, string> GetPrintableStatDictionary()
    {
        throw new NotSupportedException();
    }

    public void ApplyStatModification(PlayerCharacter playerCharacter)
    {
        int currentMaxHealth = playerCharacter.CurrentStats.maxHealth;

        playerCharacter.SetCurrentStats(new CharacterStats(playerCharacter.CurrentStats, maxHealth: currentMaxHealth + this.HealthOffset));
        playerCharacter.TakeDamage(HealthOffset, DamageType.healing);

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
    #endregion
}
