using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerCharacter : NetworkBehaviour
{
    #region Editor vars

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private IntGameEventSO onCharacterDeath;

    [SerializeField]
    private IntGameEventSO onCharacterResurrect;

    #endregion

    #region Sync vars

    [SyncVar(hook = nameof(OnCharClassIDChanged))]
    public int charClassID;
    public CharacterClass charClass;
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
    [SyncVar]
    public int ownerID;
    [SyncVar]
    public bool isKing;
    [SyncVar]
    private bool isDead;
    #endregion

    #region Startup

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.OnCharClassIDChanged(-1, this.charClassID);
    }

    //Called when char class is set in callback
    [Command(requiresAuthority = false)]
    private void CmdInitCharacter(NetworkConnectionToClient sender = null)
    {
        this.currentStats = this.charClass.stats;
        if (this.isKing)
            this.currentStats = new CharacterStats(this.currentStats, maxHealth: Utility.ApplyKingLifeBuff(this.currentStats.maxHealth));

        this.isDead = false;
        this.currentLife = this.currentStats.maxHealth;
        this.equippedTreasureIDs = new();
        this.hasUsedAbility = false;
        this.hasUsedTreasure = false;
        this.ResetTurnState();
    }

    #endregion

    #region Callbacks

    [Client]
    private void OnCharClassIDChanged(int _, int newID)
    {
        this.charClass = null;

        if (ClassDataSO.Singleton.GetClassByID(this.charClassID) != null)
        {
            this.charClass = ClassDataSO.Singleton.GetClassByID(this.charClassID);
            this.CmdInitCharacter();
        }
        else
        {
            Debug.Log("Couldn't find corresponding class for classID");
        }
    }

    #endregion

    #region State management

    [Server]
    public void SetOwner(int ownerID)
    {
        this.ownerID = ownerID;
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
            Debug.LogFormat("Attempting to attack with {0} while it has already attacked. You should validate attack beforehand.", this.charClass.name);
            return;
        }
        if (this.hasMoved)
            this.remainingMoves = 0;
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
    public void TakeDamage(int damageAmount, DamageType damageType, bool bypassArmor = false)
    {
        switch (damageType)
        {
            case DamageType.normal:
                if(!bypassArmor)
                    this.TakeRawDamage(damageAmount - this.currentStats.armor);
                else
                    this.TakeRawDamage(damageAmount);
                break;
            case DamageType.magic:
                this.TakeRawDamage(damageAmount);
                break;
            case DamageType.healing:
                this.TakeRawDamage(-damageAmount);
                break;
        }
    }

    [Server]
    private void TakeRawDamage(int damageAmount)
    {
        this.currentLife = Mathf.Clamp(currentLife - damageAmount, 0, this.currentStats.maxHealth);
        if(this.currentLife == 0)
        {
            this.Die();
        }
    }

    [Server]
    public void Die()
    {
        this.isDead = true;
        this.RpcOnCharacterDeath();
    }

    [Server]
    public void Resurrect()
    {
        this.isDead = false;
        this.RpcOnCharacterResurrect();        
    }

    //update position on all clients
    [ClientRpc]
    public void RpcPlaceChar(Vector3 position)
    {
        this.transform.position = position;
    }
    #endregion

    #region Events
    [ClientRpc]
    private void RpcOnCharacterDeath()
    {
        this.onCharacterDeath.Raise(this.charClassID);
    }

    [ClientRpc]
    private void RpcOnCharacterResurrect()
    {
        this.onCharacterResurrect.Raise(this.charClassID);
    }

    public void OnCharacterDeath(int classID)
    {
        if(classID == this.charClassID)
            this.spriteRenderer.color = Utility.GrayOutColor(this.spriteRenderer.color, true);
    }

    public void OnCharacterResurrect(int classID)
    {
        if(classID == this.charClassID)
            this.spriteRenderer.color = Utility.GrayOutColor(this.spriteRenderer.color, false);
    }
    #endregion

    #region Utility
    public bool HasRemainingActions()
    {
        if (this.hasMoved && this.hasAttacked && this.hasUsedAbility && this.hasUsedTreasure)
            return false;
        else
            return true;
    }

    public int CanMoveDistance() => this.remainingMoves;

    public int CurrentLife() => this.currentLife;

    public bool IsDead() => this.isDead;

    #endregion
}
