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
    [SyncVar]
    public List<uint> equippedTreasureIDs;
    [SyncVar]
    public CharacterStats currentStats;
    [SyncVar]
    public bool hasMoved;
    [SyncVar]
    public bool hasAttacked;
    [SyncVar]
    public bool hasUsedAbility;
    [SyncVar]
    public bool hasUsedTreasure;
    [SyncVar]
    private int remainingMoves;

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.OnCharClassIDChanged(-1, this.charClassID);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    [Client]
    private void OnCharClassIDChanged(int _, int newID)
    {
        this.charClass = null;

        if (GameController.Singleton.characterClassesByID[this.charClassID] != null)
        {
            this.charClass = GameController.Singleton.characterClassesByID[this.charClassID];
            this.CmdInitCharacter();
        }
        else
        {
            Debug.Log("Couldn't find corresponding class for classID");
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdInitCharacter()
    {
        this.currentStats = this.charClass.stats;
        this.currentLife = this.currentStats.maxHealth;
        equippedTreasureIDs = new();
        hasUsedAbility = false;
        hasUsedTreasure = false;
        this.ResetTurnState();
    }

    public bool HasRemainingActions()
    {
        if (this.hasMoved && this.hasAttacked && this.hasUsedAbility && this.hasUsedTreasure)
            return false;
        else
            return true;
    }
    public int CanMoveDistance()
    {
        if (hasMoved && (hasAttacked || hasUsedAbility || hasUsedTreasure))
            return 0;
        else
            return remainingMoves;
    }

    public int CurrentLife()
    {
        return this.currentLife;
    }

    [Server]
    public void UseMoves(int moveDistance)
    {
        if (this.CanMoveDistance() < moveDistance)
        {
            Debug.LogFormat("Attempting to move {0} by {1} while it only has {2} moves left. You should validate move beforehand.", this.charClass.name, moveDistance, this.CanMoveDistance());
            return;
        }
        this.remainingMoves -= moveDistance;
        this.hasMoved = true;
    }

    [Server]
    public void UseAttack()
    {
        if (this.hasAttacked)
        {
            Debug.LogFormat("Attempting to attack with {0} while it has already attacked. You should validate move beforehand.", this.charClass.name);
            return;
        }
        this.hasAttacked = true;
    }

    [Server]
    public void ResetTurnState()
    {
        this.hasMoved = false;
        this.hasAttacked = false;
        this.remainingMoves = this.currentStats.moveSpeed;
    }

    [Server]
    public void TakeRawDamage(int dmgAmount)
    {
        this.currentLife = Mathf.Clamp(currentLife - dmgAmount, 0, this.currentStats.maxHealth);        
    }    
}
