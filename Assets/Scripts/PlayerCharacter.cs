using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCharacter : NetworkBehaviour
{
    //TODO : charability should be stored via ID (enum type) for syncvar purposes
    public CharacterClass CharClass;
    [SyncVar]
    private int currentLife;
    [SyncVar]
    public List<TreasureID> EquippedTreasure;
    [SyncVar]
    public CharacterStats CurrentStats;
    [SyncVar]
    public string className;
    [SyncVar]
    public bool hasMoved = false;
    [SyncVar]
    public bool hasAttacked = false;
    [SyncVar]
    public bool hasUsedAbility = false;
    [SyncVar]
    public bool hasUsedTreasure = false;


    public int CurrentLife
    {
        get => currentLife;
        set
        {
            if (value < 0)
            {
                this.currentLife = 0;
            }
            else if (value > CurrentStats.maxHealth)
            {
                this.currentLife = this.CurrentStats.maxHealth;
            }
        }
    }

    public void Initialize(CharacterClass charChlass) {
        this.CharClass = charChlass;
        this.CurrentStats = charChlass.charStats;
        this.CurrentLife = CurrentStats.maxHealth;
    }

   public bool HasRemainingActions()
    {
        if (this.hasMoved && this.hasAttacked && this.hasUsedAbility && this.hasUsedTreasure)
            return false;
        else
            return true;
    }

    public void NextTurn()
    {
        this.hasMoved = false;
        this.hasAttacked = false;
    }
}
