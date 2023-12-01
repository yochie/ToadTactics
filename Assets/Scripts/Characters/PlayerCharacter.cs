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
    private IntGameEventSO onCharacterDeathServerSide;

    [SerializeField]
    private IntGameEventSO onCharacterHitServerSide;

    [SerializeField]
    private HitIntGameEventSO onCharacterTakesHit;

    [SerializeField]
    private IntGameEventSO onCharacterResurrect;

    [SerializeField]
    private IntIntIntGameEventSO onCharacterLifeChanged;

    [SerializeField]
    private Color kingColor;
    
    [SerializeField]
    private float movementAnimationDurationSeconds;


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
    private int ballistaUseCountThisTurn;
    public int BallistaUseCountThisTurn => this.ballistaUseCountThisTurn;
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

    public List<RuntimeBuff> ownerOfBuffs;
    public List<RuntimeBuff> affectedByBuffs;
    private ListWithDuplicates<RuntimeBuff, Component> buffListeners = new();
    #endregion

    #region Clienside vars
    //used to restore color after animations
    private Color baseColor;
    public Color BaseColor => this.baseColor;
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
    internal void UsedBallista()
    {
        if (!this.HasAvailableBallista())
        {
            Debug.LogFormat("Attempting to use ballista with {0} while it isn't available. You should validate attack beforehand.", this.charClass.name);
            return;
        }

        this.ballistaUseCountThisTurn++;
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
        this.ballistaUseCountThisTurn = 0;
        this.remainingMoves = this.CurrentStats.moveSpeed;
    }

    [Server]
    public void TakeDamage(Hit hit)
    {
        int rawDamage = this.CalculateDamageFromHit(hit);
        this.TakeRawDamage(rawDamage);

        if (hit.damageType != DamageType.healing)
        {
            this.onCharacterHitServerSide.Raise(this.charClassID);           
        }

        this.RpcOnCharacterTakesHit(hit, this.charClassID);
    }

    [Server]
    public int CalculateDamageFromHit(Hit hit)
    {
        List<IMitigationEnhancer> orderedMitigationEnhancers = this.GetOrderedMitigationEnhancers();

        foreach (IMitigationEnhancer mitigationEnhancer in orderedMitigationEnhancers)
        {
            hit = mitigationEnhancer.MitigateHit(hit);
        }

        switch (hit.damageType)
        {
            case DamageType.physical:
                if (!hit.penetratesArmor)
                {
                    int mitigatedDamage = Math.Max(hit.damage - this.CurrentStats.armor, 0);
                    return mitigatedDamage;
                } else
                    return hit.damage;
            case DamageType.magic:
                return hit.damage;
            case DamageType.healing:
                return -hit.damage;
            default:
                throw new Exception("Unexpected damage type.");
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
        string message = string.Format("{0} has died", this.charClass.name);
        MasterLogger.Singleton.RpcLogMessage(message);

        Map.Singleton.SetCharacterAliveState(this.charClassID, isDead: true);
        this.RemoveBuffsOnDeath();

        this.RpcOnCharacterDeath();
        //Need to throw on server syncronously because triggers some game logic stuff (triggered buffs)
        //TODO : find more elegant solution to having server only events...
        this.onCharacterDeathServerSide.Raise(this.charClassID);
    }

    [Server]
    private void RemoveBuffsOnDeath()
    {
        BuffManager.Singleton.RemoveBuffsOnDeath(this);
    }

    [Server]
    internal void RemoveOwnedBuff(RuntimeBuff buff)
    {
        this.ownerOfBuffs.Remove(buff);
    }

    [Server]
    internal void RemoveAffectingBuff(RuntimeBuff buff)
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
        MasterLogger.Singleton.RpcLogMessage(string.Format("{0} has been resurrected.", this.charClass.name));
    }

    public void PlaceCharSprite(Vector3 position, bool slide)
    {
        if (slide)
            AnimationSystem.Singleton.Queue(this.SlideToPosition(position, this.movementAnimationDurationSeconds));
        else
            this.transform.position = position;
    }

    private IEnumerator SlideToPosition(Vector3 position, float animationDurationSeconds)
    {
        float elapsedSeconds = 0f;

        while (elapsedSeconds < animationDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.transform.position = Vector3.Lerp(this.transform.position, position, elapsedSeconds / animationDurationSeconds);
            yield return null;
        }
    }

    //update position on all clients
    [ClientRpc]
    public void RpcPlaceCharSprite(Vector3 position, bool slide)
    {

        this.PlaceCharSprite(position, slide);
        
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
        this.baseColor = this.spriteRenderer.color;
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
    internal void AddAffectingBuff(RuntimeBuff buff)
    {
        this.affectedByBuffs.Add(buff);
    }

    [Server]
    internal void AddOwnedBuff(RuntimeBuff buff)
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
    internal void ApplyAbilityBuffsForRoundStart()
    {
        foreach (CharacterAbilityStats ability in this.charClass.abilities)
        {
            if (ability.appliesSelfBuffOnRoundStart == null)
                continue;

            IBuffDataSO passiveBuffData = BuffDataSO.Singleton.GetBuffData(ability.appliesSelfBuffOnRoundStart);
            List<int> applyTo = new List<int> { this.CharClassID };
            RuntimeBuff passiveBuff = BuffManager.Singleton.CreateAbilityBuff(passiveBuffData, ability, this.CharClassID, applyTo);
            BuffManager.Singleton.ApplyNewBuff(passiveBuff);
        }
    }

    [Server]
    public void GrantMovesForTurn(int movesDelta)
    {
        this.remainingMoves += movesDelta;
    }

    [Server]
    internal void AddListenerForBuff(RuntimeBuff buff, Component toAdd)
    {
        this.buffListeners.Add(buff, toAdd);
    }

    [Server]
    internal void RemoveListenersForBuff(RuntimeBuff buff)
    {
        List<Component> toDestroy = this.buffListeners.GetValues(buff);
        for(int i = toDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(toDestroy[i]);
        }
        this.buffListeners.RemoveAllValuesForKey(buff);
    }

    [Server]
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

    [ClientRpc]
    private void RpcOnCharacterTakesHit(Hit hit, int charClassID)
    {
        this.onCharacterTakesHit.Raise(hit, charClassID);
    }

    //handles character flashing effect
    public void OnCharactacterTakesHit(Hit hit, int charClassID)
    {

    }

    #endregion

    #region Utility

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
        if (this.HasAvailableBallista())
            activeControlModes.Add(ControlMode.useBallista);

        return activeControlModes;
    }

    public bool HasAvailableMoves()
    {
        if (this.RemainingMoves > 0 && this.canMove)
            return true;
        else
            return false;
    }
    public bool HasRemainingActions()
    {
        if (this.HasAvailableMoves() || this.HasAvailableAttacks() || this.HasAvailableAbilities() || this.HasAvailableActivatedEquipments() || this.HasAvailableBallista())
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
        if (this.AttackCountThisTurn >= this.currentStats.attacksPerTurn || this.BallistaUseCountThisTurn > 0)
            return false;
        else
            return true;
    }

    internal bool HasAvailableBallista()
    {
        //TODO: add check for ballista loading
        if (!Map.Singleton.IsCharacterOnBallista(this.charClassID) || this.AttackCountThisTurn >= this.currentStats.attacksPerTurn || this.BallistaUseCountThisTurn > 0)
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

    private List<IMitigationEnhancer> GetOrderedMitigationEnhancers()
    {
        List<IMitigationEnhancer> orderedMitigators = new();
        foreach (IBuffDataSO buff in this.affectedByBuffs.Select(buff => buff.Data))
        {
            IMitigationEnhancer mitigationBuff = buff as IMitigationEnhancer;
            if (mitigationBuff != null)
            {
                orderedMitigators.Add(mitigationBuff);
            }
        }
        orderedMitigators.Sort();
        return orderedMitigators;

    }

    //TODO: add check for equipments here eventually once they exist in this category
    internal List<IAttackEnhancer> GetAttackEnhancers()
    {
        List<IAttackEnhancer> toReturn = new();
        foreach (IBuffDataSO buff in this.affectedByBuffs.Select(buff => buff.Data))
        {
            IAttackEnhancer attackEnhancer = buff as IAttackEnhancer;
            if (attackEnhancer != null)
                toReturn.Add(attackEnhancer);
        }
        return toReturn;
    }

    internal Dictionary<int, string> GetAffectingBuffDataIDs()
    {
        Dictionary<int, string> buffIDs = new();
        foreach (RuntimeBuff buff in this.affectedByBuffs)
        {
            if (buff.Data.Icon != null)
                buffIDs[buff.UniqueID] = buff.Data.stringID;
        }
        return buffIDs;
    }
    #endregion
}
