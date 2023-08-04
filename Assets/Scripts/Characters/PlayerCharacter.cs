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
    private readonly SyncList<string> equipmentIDs = new();
    //TODO: serialized charClassID might be an issue according to best practices documented here : https://unitystation.github.io/unitystation/development/SyncVar-Best-Practices-for-Easy-Networking/
    //TODO : consider changing to seperate field used for init...
    [SyncVar(hook = nameof(OnCharClassIDChanged))]
    [SerializeField]
    private int charClassID;
    public int CharClassID => this.charClassID;
    public CharacterClass charClass;
    [SyncVar]
    private int currentLife;
    public int CurrentLife => this.currentLife;
    [SyncVar]
    private CharacterStats currentStats;
    public CharacterStats CurrentStats { get => this.currentStats; }
    [SyncVar]
    private bool hasMoved;
    public bool HasMoved => this.hasMoved;
    [SyncVar]
    private bool hasAttacked;
    public bool HasAttacked => this.hasAttacked;
    [SyncVar]
    private bool hasUsedAbility;
    public bool HasUsedAbility => this.hasUsedAbility;
    [SyncVar]
    private bool hasUsedAllEquipments;
    [SyncVar]
    private int remainingMoves;
    public int RemainingMoves => this.remainingMoves;
    [SyncVar]
    private int ownerID;
    public int OwnerID => this.ownerID;
    [SyncVar]
    private bool isKing;
    public bool IsKing { get => this.isKing;}
    [SyncVar]
    private bool isDead;
    public bool IsDead { get => this.isDead; }

    [SyncVar]
    private bool canMove;
    public bool CanMove { get => this.canMove;}

    [SyncVar]
    private bool canTakeTurns;
    public bool CanTakeTurns { get => this.canTakeTurns; }

    #endregion

    #region Server only vars
    public List<IBuffEffect> appliedBuffs;
    public List<IBuffEffect> affectingBuffs;
    #endregion

    #region Startup

    public override void OnStartServer()
    {
        base.OnStartServer();
        appliedBuffs = new();
        affectingBuffs = new();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.OnCharClassIDChanged(-1, this.CharClassID);
    }

    //Called when char class is set in callback
    [Server]
    private void InitCharacterSyncvars(NetworkConnectionToClient sender = null)
    {
        this.canTakeTurns = true;
        this.canMove = true;
        this.SetCurrentStats(this.charClass.stats);

        this.currentLife = this.CurrentStats.maxHealth;

        foreach (string equipmentID in this.equipmentIDs)
        {
            this.ApplyEquipment(equipmentID);
        }

        if (this.IsKing)
            this.SetCurrentStats(new CharacterStats(this.CurrentStats, maxHealth: Utility.ApplyKingLifeBuff(this.CurrentStats.maxHealth)));

        this.RpcOnCharacterLifeChanged(this.CurrentLife, this.CurrentStats.maxHealth);

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

        if (ClassDataSO.Singleton.GetClassByID(this.CharClassID) != null)
        {
            this.charClass = ClassDataSO.Singleton.GetClassByID(this.CharClassID);
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
    public void SetOwner(int value)
    {
        this.ownerID = value;
    }

    [Server]
    public void SetKing(bool value)
    {
        this.isKing = value;
    }

    [Server]
    public void SetCanMove(bool value)
    {
        this.canMove = value;
    }

    [Server]
    public void SetCanTakeTurns(bool value)
    {
        this.canTakeTurns = value;
    }

    [Server]
    public void SetCurrentStats(CharacterStats value)
    {
        this.currentStats = value;
    }

    [Server]
    public void UseMoves(int moveDistance)
    {
        if (this.RemainingMoves < moveDistance)
        {
            Debug.LogFormat("Attempting to move {0} by {1} while it only has {2} moves left. You should validate move beforehand.", this.charClass.name, moveDistance, this.RemainingMoves);
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
        this.remainingMoves = this.CurrentStats.moveSpeed;
    }

    [Server]
    public void TakeDamage(int damageAmount, DamageType damageType, bool bypassArmor = false)
    {
        switch (damageType)
        {
            case DamageType.physical:
                if(!bypassArmor)
                    this.TakeRawDamage(damageAmount - this.CurrentStats.armor);
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
        this.currentLife = Mathf.Clamp(this.currentLife - damageAmount, 0, this.CurrentStats.maxHealth);

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

    [Server]
    internal void GiveEquipment(string equipmentID)
    {
        this.equipmentIDs.Add(equipmentID);
    }

    [Server]
    internal void AddAffectingBuff(IBuffEffect buff)
    {
        this.affectingBuffs.Add(buff);
    }

    [Server]
    internal void AddAppliedBuff(IBuffEffect buff)
    {
        this.appliedBuffs.Add(buff);
    }
    #endregion

    #region Events
    [ClientRpc]
    private void RpcOnCharacterDeath()
    {
        this.onCharacterDeath.Raise(this.CharClassID);
    }

    [ClientRpc]
    private void RpcOnCharacterResurrect()
    {
        this.onCharacterResurrect.Raise(this.CharClassID);
    }

    public void OnCharacterDeath(int classID)
    {
        if(classID == this.CharClassID)
            this.spriteRenderer.color = Utility.GrayOutColor(this.spriteRenderer.color, true);
    }

    public void OnCharacterResurrect(int classID)
    {
        if(classID == this.CharClassID)
            this.spriteRenderer.color = Utility.GrayOutColor(this.spriteRenderer.color, false);
    }

    //Careful not to call several times in quick succession, order of rpc calls is unreliable in that case
    //means we can't use in TakeDamage, instead we use after whole attack (in action executor) and after char full init
    [ClientRpc]
    public void RpcOnCharacterLifeChanged(int currentLife, int maxHealth)
    {
        this.onCharacterLifeChanged.Raise(this.CharClassID, currentLife, maxHealth);
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
    #endregion
}
