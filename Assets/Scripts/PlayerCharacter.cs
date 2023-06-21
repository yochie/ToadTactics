using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerCharacter : NetworkBehaviour
{
    public CharacterClass charClass;

    [SyncVar(hook = nameof(OnCharClassIDChanged))]
    public int charClassID;
    [SyncVar]
    private int currentLife;
    //treasureID
    [SyncVar]
    public List<uint> equippedTreasureIDs;
    [SyncVar]
    public CharacterStats currentStats;
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
            else if (value > currentStats.maxHealth)
            {
                this.currentLife = this.currentStats.maxHealth;
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.OnCharClassIDChanged(-1, this.charClassID);
    }

    private void OnCharClassIDChanged(int _, int newID)
    {
        this.charClass = null;

        if (GameController.Singleton.AllClasses[this.charClassID] != null)
            this.charClass = GameController.Singleton.AllClasses[this.charClassID];
        else
            Debug.Log("Couldn't find corresponding class for classID");
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
