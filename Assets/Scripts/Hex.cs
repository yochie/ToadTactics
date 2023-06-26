using System;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class Hex : NetworkBehaviour, IEquatable<Hex>, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    #region Constants

    private static readonly Color HEX_DEFAULT_COLOR = Color.white;
    private static readonly Color HEX_START_COLOR = Color.blue;
    private static readonly Color HEX_OPPONENT_START_COLOR = Color.grey;
    private static readonly Color HEX_HOVER_COLOR = Color.cyan;
    private static readonly Color HEX_SELECT_COLOR = Color.green;
    private static readonly Color HEX_RANGE_COLOR = new(0.6940628f, 0.9433962f, 0.493058f);
    private static readonly Color HEX_LOS_OBSTRUCT_COLOR = Color.yellow;
    private static readonly Color HEX_ATTACK_HOVER_COLOR = new(1f, 0.45f, 0f);
    private static readonly Color HEX_ATTACK_RANGE_COLOR = new(0.6940628f, 0.9433962f, 0.493058f);
    private static readonly Color HEX_OUT_OF_ATTACK_RANGE_COLOR = Color.red;


    #endregion

    #region UI vars

    private SpriteRenderer sprite;

    public Color baseColor = Hex.HEX_DEFAULT_COLOR;
    public Color unHoveredColor = Hex.HEX_DEFAULT_COLOR;
    public Color hexColor = Hex.HEX_DEFAULT_COLOR;
    private Color HexColor {
        get { return this.hexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
        }
    }
    private TextMeshProUGUI coordLabelTextMesh;
    private TextMeshProUGUI labelTextMesh;
    private string labelString;
    public string LabelString {
        get { return labelString;  }
        set { 
            labelString = value;
            this.labelTextMesh.text = value;
        }
    }

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
        this.baseColor = Hex.HEX_DEFAULT_COLOR;
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

        //Debug.LogFormat("Creating hex on client {0} {1} {2}",this.coordinates, this.coordinates.X, this.coordinates.Y);
        this.sprite = this.GetComponent<SpriteRenderer>();

        this.InitBaseColor();

        //coordinates hidden by default using canvas group alpha
        //use that component in editor mode to display
        TextMeshProUGUI coordLabel = Instantiate<TextMeshProUGUI>(Map.Singleton.cellLabelPrefab);
        coordLabel.rectTransform.SetParent(Map.Singleton.coordCanvas.transform, false);
        coordLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        coordLabel.text = this.coordinates.X + " " + this.coordinates.Y;
        this.coordLabelTextMesh = coordLabel;


        //labels to display single number during navigation (range, etc)
        TextMeshProUGUI numLabel = Instantiate<TextMeshProUGUI>(Map.Singleton.cellLabelPrefab);
        numLabel.fontSize = 4;
        numLabel.rectTransform.SetParent(Map.Singleton.labelsCanvas.transform, false);
        numLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        this.labelTextMesh = numLabel;

        if (isClientOnly)
            this.CmdMarkAsSpawned();
    }

    private void InitBaseColor()
    {
        if (this.isStartingZone)
        {
            if ((this.isServer && this.startZoneForPlayerIndex == 0) || (!this.isServer && this.startZoneForPlayerIndex == 1))
            {
                this.baseColor = Hex.HEX_START_COLOR;

            }
            else
            {
                this.baseColor = Hex.HEX_OPPONENT_START_COLOR;
            }
        } else
        {

            //should already be set in basic Init, but just to be sure...
            this.baseColor = Hex.HEX_DEFAULT_COLOR;
        }

        this.HexColor = this.baseColor;
        this.unHoveredColor = this.baseColor;
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
        if (this.coordLabelTextMesh != null)
        {
            Destroy(this.coordLabelTextMesh.gameObject);
        }

        if (this.labelTextMesh != null)
        {
            Destroy(this.labelTextMesh.gameObject);
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

    #region UI state

    internal void ShowLabel()
    {
        this.labelTextMesh.alpha = 1;
    }

    internal void HideLabel()
    {
        this.labelTextMesh.alpha = 0;
    }

    public void Select(bool mode)
    {
        this.unHoveredColor = mode ? Hex.HEX_SELECT_COLOR : this.baseColor;
        this.HexColor = mode ? Hex.HEX_SELECT_COLOR : this.baseColor;
    }

    public void MoveHover(bool mode) {
        if (mode) { this.unHoveredColor = this.HexColor; }
        this.HexColor = mode ? Hex.HEX_HOVER_COLOR : this.unHoveredColor;
    }

    public void DisplayMoveRange(bool mode)
    {
        this.unHoveredColor = mode ? Hex.HEX_RANGE_COLOR : this.baseColor;
        this.HexColor = mode ? Hex.HEX_RANGE_COLOR : this.baseColor;
    }
    public void DisplayAttackRange(bool mode)
    {
        this.unHoveredColor = mode ? Hex.HEX_ATTACK_RANGE_COLOR : this.baseColor;
        this.HexColor = mode ? Hex.HEX_ATTACK_RANGE_COLOR : this.baseColor;
    }

    public void DisplayLOSObstruction(bool mode)
    {
        this.unHoveredColor = mode ? Hex.HEX_LOS_OBSTRUCT_COLOR : this.baseColor;
        this.HexColor = mode ? Hex.HEX_LOS_OBSTRUCT_COLOR : this.baseColor;
    }

    internal void DisplayOutOfAttackRange(bool mode)
    {
        this.unHoveredColor = mode ? Hex.HEX_OUT_OF_ATTACK_RANGE_COLOR : this.baseColor;
        this.HexColor = mode ? Hex.HEX_OUT_OF_ATTACK_RANGE_COLOR : this.baseColor;
    }

    public void AttackHover(bool mode)
    {
        if (mode) { this.unHoveredColor = this.HexColor; }
        this.HexColor = mode ? Hex.HEX_ATTACK_HOVER_COLOR : this.unHoveredColor;
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
        switch (GameController.Singleton.currentGamePhase)
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