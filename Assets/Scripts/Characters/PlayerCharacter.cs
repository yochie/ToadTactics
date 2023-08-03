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

    [SerializeField]
    private IntIntIntGameEventSO onCharacterLifeChanged;
    #endregion

    #region Sync vars
    public CharacterClass charClass;

    private readonly SyncList<string> equipmentIDs = new();
    [SyncVar(hook = nameof(OnCharClassIDChanged))]
    public int charClassID;
    [SyncVar]
    private int currentLife;
    [SyncVar]
    public CharacterStats currentStats;
    [SyncVar]
    public bool hasMoved;
    [SyncVar]
    public bool hasAttacked;
    [SyncVar]
    public bool hasUsedAbility;
    [SyncVar]
    public bool hasUsedAllEquipments;
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
    [Server]
    private void InitCharacterSyncvars(NetworkConnectionToClient sender = null)
    {
        this.currentStats = this.charClass.stats;

        this.currentLife = this.currentStats.maxHealth;

        foreach (string equipmentID in this.equipmentIDs)
        {
            this.ApplyEquipment(equipmentID);
        }

        if (this.isKing)
            this.currentStats = new CharacterStats(this.currentStats, maxHealth: Utility.ApplyKingLifeBuff(this.currentStats.maxHealth));

        this.RpcOnCharacterLifeChanged(this.CurrentLife(), this.currentStats.maxHealth);

        this.isDead = false;
        this.hasUsedAbility = false;
        this.hasUsedAllEquipments = false;
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
            if(isServer)
                this.InitCharacterSyncvars();
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
            case DamageType.physical:
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
        this.currentLife = Mathf.Clamp(this.currentLife - damageAmount, 0, this.currentStats.maxHealth);

        if (this.currentLife == 0)
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

    [Server]
    internal void ApplyEquipment(string equipmentID)
    {
        
        EquipmentSO equipment = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);
        Debug.LogFormat("Applying equipment {0} to {1}", equipment.NameUI, this.charClass.name);
        //call apply method if stat equipment
        IStatModifier statEquipment = equipment as IStatModifier;
        if (statEquipment != null)
        {
            statEquipment.ApplyStatModification(this);
        }
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

    //Careful not to call several times in quick succession, order of rpc calls is unreliable in that case
    //means we can't use in TakeDamage, instead we use after whole attack (in action executor) and after char full init
    [ClientRpc]
    public void RpcOnCharacterLifeChanged(int currentLife, int maxHealth)
    {
        this.onCharacterLifeChanged.Raise(this.charClassID, currentLife, maxHealth);
    }
    #endregion

    #region Utility
    public bool HasRemainingActions()
    {
        if (this.hasMoved && this.hasAttacked && this.hasUsedAbility && this.hasUsedAllEquipments)
            return false;
        else
            return true;
    }

    public int CanMoveDistance() => this.remainingMoves;

    public int CurrentLife() => this.currentLife;

    public bool IsDead() => this.isDead;

    internal void GiveEquipment(string equipmentID)
    {
        this.equipmentIDs.Add(equipmentID);
    }


    #endregion
}
