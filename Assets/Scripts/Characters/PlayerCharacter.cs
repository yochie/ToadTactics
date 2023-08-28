using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

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

    [SerializeField]
    private Color kingColor;
    #endregion

    #region Sync vars
    private readonly SyncList<string> equipmentIDs = new();
    public List<string> EquipmentIDsCopy => this.equipmentIDs.ToList();
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
    private int attackCountThisTurn;
    public int AttackCountThisTurn => this.attackCountThisTurn;
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

    private readonly SyncDictionary<string, int> abilityCooldowns = new();

    private readonly SyncDictionary<string, int> abilityUsesThisRound = new();
    #endregion

    #region Server only vars

    public List<IAbilityBuffEffect> ownerOfBuffs;
    public List<IBuff> affectedByBuffs;
    #endregion

    #region Startup

    public override void OnStartServer()
    {
        base.OnStartServer();
        ownerOfBuffs = new();
        affectedByBuffs = new();
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
        this.isDead = false;
        this.SetCurrentStats(this.charClass.stats);

        this.currentLife = this.CurrentStats.maxHealth;

        foreach (string equipmentID in this.equipmentIDs)
        {
            this.ApplyEquipment(equipmentID);
        }

        if (this.IsKing)
        {
            this.SetCurrentStats(new CharacterStats(this.CurrentStats, maxHealth: Utility.ApplyKingLifeBuff(this.CurrentStats.maxHealth)));
            this.currentLife = currentStats.maxHealth;
        }

        foreach (CharacterAbilityStats ability in this.charClass.abilities)
        {
            //passive abilities are applied in phase init
            if (ability.isPassive)
                continue;

            if(ability.cappedByCooldown)
                this.abilityCooldowns.Add(ability.stringID, 0);

            if(ability.cappedPerRound)
                this.abilityUsesThisRound.Add(ability.stringID, 0);
        }
            
        this.RpcOnCharacterLifeChanged(this.CurrentLife, this.CurrentStats.maxHealth);
        this.ResetTurnState();
    }

    //TODO: add check for equipments here eventually once they exist in this category
    internal List<IAttackEnhancer> GetAttackEnhancers()
    {
        List<IAttackEnhancer> toReturn = new();
        foreach(IBuff buff in this.affectedByBuffs)
        {
            IAttackEnhancer attackEnhancer = buff as IAttackEnhancer;
            if (attackEnhancer != null)
                toReturn.Add(attackEnhancer);
        }
        return toReturn;
    }

    internal Dictionary<int, string> GetAffectingBuffIcons()
    {
        Dictionary<int, string> buffIcons = new();
        foreach(IBuff buff in this.affectedByBuffs)
        {
            IDisplayedBuff displayedBuff = buff as IDisplayedBuff;
            if(displayedBuff != null)
                buffIcons[displayedBuff.UniqueID] = displayedBuff.IconName;
        }
        return buffIcons;
    }

    internal void SetCurrentLife(int value)
    {
        int previousHealth = this.CurrentLife;
        this.currentLife = Mathf.Clamp(value, 0, this.currentStats.maxHealth);
        if (previousHealth > 0 && this.CurrentLife <= 0 && !this.isDead)
        {
            this.Die();
        }
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
    public void UsedMoves(int moveDistance)
    {
        if (this.RemainingMoves < moveDistance || !this.canMove)
        {
            Debug.LogFormat("Attempting to move {0} by {1} while it only has {2} moves left. You should validate move beforehand.", this.charClass.name, moveDistance, this.RemainingMoves);
            return;
        }
        this.remainingMoves -= moveDistance;
    }

    [Server]
    public void UsedAttack()
    {
        if (!this.HasAvailableAttacks())
        {
            Debug.LogFormat("Attempting to attack with {0} while it has already attacked. You should validate attack beforehand.", this.charClass.name);
            return;
        }

        this.attackCountThisTurn++;
    }

    [Server]
    public void UsedAbility(string abilityID)
    {
        CharacterAbilityStats ability = this.GetAbilityWithID(abilityID);
        if (ability.cappedByCooldown)
        {
            this.abilityCooldowns[abilityID] = this.GetAbilityWithID(abilityID).cooldownDuration + 1;

        }

        if (ability.cappedPerRound)
        {
            this.abilityUsesThisRound[abilityID]++;
        }

        if (!this.HasAvailableAbilities())
        {
            MainHUD.Singleton.TargetRpcGrayOutAbilityButton(target: GameController.Singleton.GetConnectionForPlayerID(this.ownerID));
        }
    }

    [Server]
    public void ResetTurnState()
    {
        this.attackCountThisTurn = 0;
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

    //Don't check for round end here since we don't want to keep context on call stack when ending round.
    //instead check for round end wherever characters can take damage (end of actions, end of turn, start of turn)
    [Server]
    public void Die()
    {
        this.isDead = true;
        this.currentLife = 0;
        this.ResetCooldownsAndUses();
        this.RemoveRoundBuffs();
        this.RpcOnCharacterDeath();

        string message = string.Format("{0} has died", this.charClass.name);
        MasterLogger.Singleton.RpcLogMessage(message);

        Map.Singleton.SetCharacterAliveState(this.charClassID, isDead: true);
    }

    [Server]
    private void RemoveRoundBuffs()
    {
        BuffManager.Singleton.RemoveRoundBuffsAppliedToCharacter(this);
    }

    [Server]
    internal void RemoveOwnedBuff(IAbilityBuffEffect buff)
    {
        this.ownerOfBuffs.Remove(buff);
    }

    [Server]
    internal void RemoveAffectingBuff(IBuff buff)
    {
        this.affectedByBuffs.Remove(buff);
    }

    [Server]
    private void ResetCooldownsAndUses()
    {
        foreach (CharacterAbilityStats ability in this.charClass.abilities)
        {
            string abilityID = ability.stringID;
            if(ability.cappedByCooldown)
                this.abilityCooldowns[abilityID] = 0;
            if(ability.cappedPerRound)
                this.abilityUsesThisRound[abilityID] = 0;
        }
    }

    [Server]
    public void Resurrect(int lifeOnResurrection)
    {
        this.isDead = false;
        this.currentLife = Mathf.Clamp(lifeOnResurrection, 0, this.currentStats.maxHealth);
        Map.Singleton.SetCharacterAliveState(this.charClassID, isDead: false);
        this.RpcOnCharacterResurrect();
    }

    //update position on all clients
    [ClientRpc]
    public void RpcPlaceCharSprite(Vector3 position)
    {
        this.transform.position = position;
    }

    [ClientRpc]
    public void RpcPlaceAndSetVisible(bool visibleState, Vector3 position)
    {
        this.spriteRenderer.enabled = visibleState;
        if (this.isKing)
        {
            this.spriteRenderer.color = this.kingColor;
        }
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
    internal void AddAffectingBuff(IBuff buff)
    {
        this.affectedByBuffs.Add(buff);
    }

    [Server]
    internal void AddOwnedBuff(IAbilityBuffEffect buff)
    {
        this.ownerOfBuffs.Add(buff);
    }

    [Server]
    public void TickCooldownsForTurn()
    {
        foreach(KeyValuePair<string, int> cooldown in this.abilityCooldowns.ToList())
        {
            if(cooldown.Value > 0)
                this.abilityCooldowns[cooldown.Key]--;
        }
    }

    [Server]
    internal void ApplyPassiveAbilityBuffs()
    {
        foreach (CharacterAbilityStats ability in this.charClass.abilities)
        {
            if (!ability.isPassive)
                continue;

            Type passiveBuffType = ClassDataSO.Singleton.GetBuffTypesByPassiveAbilityID(ability.stringID);
            List<int> applyTo = new List<int> { this.CharClassID };
            IAbilityBuffEffect passiveBuff = BuffManager.Singleton.CreateAbilityBuff(passiveBuffType, ability, this.CharClassID, applyTo);
            BuffManager.Singleton.ApplyNewBuff(passiveBuff);
        }
    }

    [Server]
    public void GrantMovesForTurn(int movesDelta)
    {
        this.remainingMoves += movesDelta;
    }

    #endregion

    #region Events
    [ClientRpc]
    private void RpcOnCharacterDeath()
    {
        this.onCharacterDeath.Raise(this.CharClassID);
    }

    internal List<ControlMode> GetRemainingActions()
    {
        List<ControlMode> activeControlModes = new();
        if (this.HasAvailableMoves())
            activeControlModes.Add(ControlMode.move);
        if (this.HasAvailableAttacks())
            activeControlModes.Add(ControlMode.attack);
        if (this.HasAvailableAbilities())
            activeControlModes.Add(ControlMode.useAbility);
        if (this.HasAvailableActivatedEquipments())
            activeControlModes.Add(ControlMode.useEquipment);

        return activeControlModes;
    }

    public bool HasAvailableMoves()
    {
        if (this.RemainingMoves > 0 && this.canMove)
            return true;
        else
            return false;
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
        if (this.HasAvailableMoves() || this.HasAvailableAttacks() || this.HasAvailableAbilities() || this.HasAvailableActivatedEquipments())
            return true;
        else
            return false;
    }

    public bool HasAvailableActivatedEquipments()
    {
        return false;
    }

    public bool HasAvailableAbilities()
    {
        foreach(CharacterAbilityStats ability in this.charClass.abilities)
        {
            if (ability.isPassive)
                continue;
            string abilityID = ability.stringID;
            bool isAvailable = true;
            if (this.AbilityOnCooldown(abilityID) || this.AbilityUsesPerRoundExpended(abilityID))
                isAvailable = false;            
            if (isAvailable)
                return true;
        }
        return false;
    }

    internal bool HasAvailableAttacks()
    {
        if (this.AttackCountThisTurn >= this.currentStats.attacksPerTurn)
            return false;
        else
            return true;
    }


    internal bool AbilityUsesPerRoundExpended(string abilityID)
    {
        CharacterAbilityStats ability = this.GetAbilityWithID(abilityID);
        
        //-1 means infinite uses
        if (ability.usesPerRound == -1 || !ability.cappedPerRound)
            return false;

        if (this.abilityUsesThisRound[abilityID] >= ability.usesPerRound)
            return true;
        else
            return false;
    }

    internal bool AbilityOnCooldown(string abilityID)
    {
        CharacterAbilityStats ability = this.GetAbilityWithID(abilityID);

        //0 = no cooldown, probably limited by uses per round instead
        if (ability.cooldownDuration == 0 || !ability.cappedByCooldown)
            return false;

        if (this.abilityCooldowns[abilityID] > 0)
            return true;
        else
            return false;
    }

    private CharacterAbilityStats GetAbilityWithID(string abilityID)
    {
        return this.charClass.abilities.Single(ability => ability.stringID == abilityID);
    }

    internal int GetAbilityCooldown(string abilityID)
    {
        CharacterAbilityStats abilityStats = this.GetAbilityWithID(abilityID);
        if (!abilityStats.cappedByCooldown)
            return -1;

        return this.abilityCooldowns[abilityID];
    }
    
    internal int GetAbilityUsesRemaining(string abilityID)
    {
        CharacterAbilityStats abilityStats = this.GetAbilityWithID(abilityID);

        if (abilityStats.usesPerRound == -1 || !abilityStats.cappedPerRound)
            return -1;

        return abilityStats.usesPerRound - this.abilityUsesThisRound[abilityID];
    }

    internal bool HasActiveAbility()
    {
        foreach (CharacterAbilityStats ability in this.charClass.abilities)
        {
            if (!ability.isPassive)
                return true;
        }
        return false;
    }
    #endregion
}
