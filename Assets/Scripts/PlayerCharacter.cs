using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCharacter : NetworkBehaviour
{
    private int currentLife;
    public CharacterClass CharClass { get; set; }
    public List<Treasure> EquippedTreasure { get; set; }
    public CharacterStats CurrentStats { get; set; }

    public string className;

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
}
