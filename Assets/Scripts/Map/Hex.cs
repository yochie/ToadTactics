using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

//RTODO: Split into seperate classes
//RTODO: IsDraggable and IsClickable moved to registered observers that do the movement/selecting

[RequireComponent(typeof(HexDrawer))]
public class Hex : NetworkBehaviour, IEquatable<Hex>, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    #region Editor vars

    [SerializeField]
    public HexDrawer drawer;
    #endregion

    #region UI vars

    private Vector3 dragStartPosition;
    private bool draggingStarted;

    #endregion

    #region Sync vars
    [SyncVar]
    public HexCoordinates coordinates;

    [SyncVar]
    public bool isStartingZone;

    [SyncVar]
    public int holdsCharacterWithClassID;

    [SyncVar]
    public ObstacleType holdsObstacle;

    [SyncVar]
    public HazardType holdsHazard;

    [SyncVar]
    public bool holdsTreasure;

    //0 is host
    //1 is client
    [SyncVar]
    public int startZoneForPlayerIndex;

    [SyncVar]
    public bool hasBeenSpawnedOnClient;
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
        this.holdsObstacle = ObstacleType.none;
        this.holdsHazard = HazardType.none;
        this.holdsTreasure = false;
        this.hasBeenSpawnedOnClient = false;

        //not currently needed as its set during instatiation, but kept in case
        //scale is needed
        this.transform.position = position;
        this.transform.localScale = scale;
        this.transform.rotation = rotation;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.drawer = this.GetComponent<HexDrawer>();
        bool isLocalStartingZone = (this.isServer && this.startZoneForPlayerIndex == 0) || (!this.isServer && this.startZoneForPlayerIndex == 1);
        this.drawer.Init(this.isStartingZone, isLocalStartingZone, this.coordinates);

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
    internal void clearCharacter()
    {
        this.holdsCharacterWithClassID = -1;
    }
    public void Delete()
    {
        if (this.drawer != null)
        {
            this.drawer.Delete();
        }

        Destroy(this.gameObject);
    }
    #endregion

    #region Events

    private void OnMouseEnter() {
        Map.Singleton.HoverHex(this);
    }

    private void OnMouseExit()
    {
        Map.Singleton.UnhoverHex(this);
    }

    void IPointerClickHandler.OnPointerClick (PointerEventData eventData)
    {
        if(IsClickable())
            Map.Singleton.ClickHex(this);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
        {
            this.dragStartPosition = this.transform.position;
            this.draggingStarted = true;
            Map.Singleton.StartDragHex(this);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (this.draggingStarted)
        {
            PlayerCharacter heldCharacter = this.GetHeldCharacterObject();
            heldCharacter.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.draggingStarted)
        {
            this.draggingStarted = false;
            PlayerCharacter heldCharacter = this.GetHeldCharacterObject();
            heldCharacter.transform.position = this.dragStartPosition;
            Map.Singleton.EndDragHex(this);
        }
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

    public PlayerCharacter GetHeldCharacterObject()
    {
        if (!this.HoldsACharacter()) {
            Debug.Log("Trying to get held character from hex without one.");
            return null; 
        }
        return GameController.Singleton.playerCharacters[this.holdsCharacterWithClassID];
    }

    public bool BreaksLOS(int targetClassID)
    {
        if ((this.HoldsACharacter() && this.holdsCharacterWithClassID != targetClassID) ||
            this.holdsObstacle != ObstacleType.none )
            return true;
        else
            return false;
    }

    //only validates local data, other checks need to be performed for allowing actual move
    public bool IsValidMoveDest()
    {
        if (this.holdsObstacle == ObstacleType.none &&
            !this.HoldsACharacter())
            return true;
        else
            return true;
    }

    public bool IsValidAttackTarget()
    {
        if (this.HoldsACharacter())
            return true;
        else
            return false;
    }

    //only validates local data, other checks need to be performed for allowing actual move
    public bool IsValidMoveSource()
    {
        if (this.HoldsACharacter())
            return true;
        else
            return false;
    }

    public bool IsValidAttackSource()
    {
        if (this.HoldsACharacter())
            return true;
        else
            return false;
    }

    public bool IsClickable()
    {
        switch (GameController.Singleton.currentPhase)
        {
            case GamePhase.characterPlacement:
                return false;
            case GamePhase.gameplay:
                switch (Map.Singleton.CurrentControlMode)
                {
                    case ControlMode.move:
                        if (this.IsValidMoveSource() &&
                            GameController.Singleton.CanIControlThisCharacter(this.holdsCharacterWithClassID))
                            return true;
                        else if (Map.Singleton.SelectedHex != null && this.IsValidMoveDest())
                            return true;
                        else
                            return false;
                    case ControlMode.attack:
                        if (this.IsValidAttackSource() &&
                            GameController.Singleton.CanIControlThisCharacter(this.holdsCharacterWithClassID))
                            return true;
                        else if (Map.Singleton.SelectedHex != null && this.IsValidAttackTarget())
                            return true;
                        else
                            return false;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    public bool IsDraggable()
    {
        switch (Map.Singleton.CurrentControlMode)
        {
            case ControlMode.move:
                if (this.IsValidMoveSource() &&
                    GameController.Singleton.CanIControlThisCharacter(this.holdsCharacterWithClassID))
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }
    public int MoveCost()
    {
        switch (this.holdsHazard)
        {
            case HazardType.cold:
                return 2;
            default:
                return 1;
        }
    }


    #endregion

}