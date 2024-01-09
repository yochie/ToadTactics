using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

[RequireComponent(typeof(HexDrawer))]
[RequireComponent(typeof(HexMouseEventTracker))]

public class Hex : NetworkBehaviour, IEquatable<Hex>
{
    //TODO: make all these private serialized fields
    #region Component vars

    public HexDrawer drawer;

    public HexMouseEventTracker inputHandler;
    
    [SerializeField]
    private SpriteRenderer mainSprite;

    [SerializeField]
    private SpriteRenderer outline1;
    
    [SerializeField]
    private SpriteRenderer outline2;   
    #endregion

    //TODO : make these private and add get/setters
    #region Sync vars
    [SyncVar]
    public HexCoordinates coordinates;

    [SyncVar]
    public bool isStartingZone;

    [SyncVar]
    public int holdsCharacterWithClassID;

    [SyncVar]
    private ObstacleType holdsObstacle;

    [SyncVar]
    public HazardType holdsHazard;

    [SyncVar]
    private bool holdsTreasure;

    [SyncVar]
    public bool holdsBallista;

    [SyncVar]
    public bool ballistaNeedsReload;

    //0 is host
    //1 is client
    [SyncVar]
    public int startZoneForPlayerIndex;

    [SyncVar]
    public bool hasBeenSpawnedOnClient;

    [SyncVar]
    public int holdsCorpseWithClassID;

    #endregion

    #region Startup

    [Server]
    public void Init(HexCoordinates hc, string name, Vector3 position, Vector3 scale, Quaternion rotation) {
        this.name = name;
        this.coordinates = hc;

        //default values
        this.isStartingZone = false;
        this.startZoneForPlayerIndex = -1;
        this.holdsCharacterWithClassID = -1;
        this.holdsCorpseWithClassID = -1;
        this.holdsObstacle = ObstacleType.none;
        this.holdsHazard = HazardType.none;
        this.holdsTreasure = false;
        this.hasBeenSpawnedOnClient = false;
        this.holdsBallista = false;
        this.ballistaNeedsReload = false;

        //not currently needed as its set during instatiation, but kept in case
        //scale is needed
        this.transform.position = position;
        this.transform.localScale = scale;
        this.transform.rotation = rotation;
    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        bool isLocalStartingZone = (this.isServer && this.startZoneForPlayerIndex == 0) || (!this.isServer && this.startZoneForPlayerIndex == 1);
        this.drawer.Init(this.isStartingZone, isLocalStartingZone, this.coordinates,this.mainSprite, this.outline1, this.outline2);

        this.inputHandler.Master = this;

        if (isClientOnly)
            this.CmdMarkAsSpawned();
    }

    [Command(requiresAuthority = false)]
    private void CmdMarkAsSpawned()
    {
        this.hasBeenSpawnedOnClient = true;
    }
    #endregion

    #region State
    [Server]
    internal void ClearCharacter()
    {
        this.holdsCharacterWithClassID = -1;
    }

    [Server]
    internal void ClearCorpse()
    {
        this.holdsCorpseWithClassID = -1;
    }

    [Server]
    internal void ClearObstacle()
    {
        this.holdsObstacle = ObstacleType.none;
    }

    public void Delete()
    {
        if (this.drawer != null)
        {
            this.drawer.Delete();
        }

        Destroy(this.gameObject);
    }

    [Server]
    internal void SetObstacle(ObstacleType obstacle)
    {
        this.holdsObstacle = obstacle;
    }

    [Server]
    public void SetTreasure(bool val)
    {
        this.holdsTreasure = val;
    }

    [Server]
    public void SetHazard(HazardType type)
    {
        this.holdsHazard = type;
    }

    [Server]
    internal void ReloadBallista()
    {
        if (!this.HoldsABallista() || !this.BallistaNeedsReload())
        {
            Debug.LogFormat("Attempting to reload ballista with {0} while it isn't available / necessary. You should validate beforehand.");
            return;
        }

        this.ballistaNeedsReload = false;
    }

    [Server]
    internal void UseBallista()
    {
        if (!this.HoldsABallista() || this.BallistaNeedsReload())
        {
            Debug.LogFormat("Attempting to use ballista with {0} while it isn't available. You should validate beforehand.");
            return;
        }

        this.ballistaNeedsReload = true;
    }
    #endregion

    #region Utility

    public bool Equals(Hex other)
    {
        return this.coordinates.Equals(other.coordinates);
    }
    public bool HoldsACharacter()
    {
        return (this.holdsCharacterWithClassID != -1);
    }

    public bool HoldsACorpse()
    {
        return (this.holdsCorpseWithClassID != -1);
    }

    internal int DealsDamageWhenMovedInto()
    {
        return HazardDataSO.Singleton.GetHazardDamage(this.holdsHazard);
    }


    internal DamageType DealsDamageTypeWhenMovedInto()
    {
        return HazardDataSO.Singleton.GetHazardDamageType(this.holdsHazard);
    }

    internal bool HoldsAnObstacle()
    {
        return this.holdsObstacle != ObstacleType.none;
    }

    public PlayerCharacter GetHeldCharacterObject()
    {
        if (!this.HoldsACharacter()) {
            Debug.Log("Trying to get held character from hex without one.");
            return null; 
        }
        return GameController.Singleton.PlayerCharactersByID[this.holdsCharacterWithClassID];
    }

    public PlayerCharacter GetHeldCorpseCharacterObject()
    {
        if (!this.HoldsACorpse())
        {
            Debug.Log("Trying to get held corpse character from hex without one.");
            return null;
        }
        return GameController.Singleton.PlayerCharactersByID[this.holdsCorpseWithClassID];
    }

    public bool BreaksLOSToTarget(Hex targetHex)
    {
        if (this.Equals(targetHex))
            return false;
        else if (this.holdsObstacle != ObstacleType.none || this.HoldsACharacter())
            return true;
        else
            return false;
    }

    public int MoveCost()
    {
        int movementPenalty = HazardDataSO.Singleton.GetHazardMovementPenalty(this.holdsHazard);
        return 1 + movementPenalty;
    }

    internal bool IsEmpty()
    {
        if (this.HoldsACharacter())
            return false;
        if (this.HoldsAnObstacle())
            return false;
        if (this.HoldsACorpse())
            return false;
        if (this.HoldsAHazard())
            return false;
        if (this.HoldsABallista())
            return false;
        if (this.HoldsATreasure())
            return false;
        return true;
    }

    public bool HoldsABallista()
    {
        return this.holdsBallista;
    }

    public bool BallistaNeedsReload()
    {
        return this.ballistaNeedsReload;
    }

    public bool HoldsAHazard()
    {
        if (this.holdsHazard != HazardType.none)
            return true;
        else
            return false;
    }

    public bool HoldsATreasure()
    {
        return this.holdsTreasure;
    }

    #endregion
}