using UnityEngine;
using TMPro;
using System;

public class HexDrawer : MonoBehaviour
{
    #region Constants
    //public static readonly Color HEX_DEFAULT_COLOR = Color.white;
    //public static readonly Color HEX_START_COLOR = Color.blue;
    //public static readonly Color HEX_OPPONENT_START_COLOR = Color.grey;
    //public static readonly Color HEX_HOVER_COLOR = Color.cyan;
    //public static readonly Color HEX_SELECT_COLOR = Color.green;
    //public static readonly Color HEX_IN_MOVE_RANGE_COLOR = new(0.6940628f, 0.9433962f, 0.493058f);
    //public static readonly Color HEX_ATTACK_TARGETABLE_COLOR = Color.red;
    //public static readonly Color HEX_ABILITY_TARGETABLE_COLOR = new(103f / 255f, 10f / 255f, 142f / 255f); 
    //public static readonly Color HEX_ATTACK_HOVER_COLOR = Color.cyan; 
    //public static readonly Color HEX_IN_ATTACK_RANGE_COLOR = new(176f / 255f, 98f / 255f, 100f / 255f);
    //public static readonly Color HEX_IN_ABILITY_RANGE_COLOR = new(189 / 255f, 111f / 255f, 221f / 255f);
    //public static readonly Color HEX_OUT_OF_ACTION_RANGE_COLOR = Color.gray;
    //public static readonly Color HEX_ABILITY_HOVER_COLOR = Color.cyan;

    //public static readonly Color OUTLINE_VALID_TARGET_COLOR = Color.green;
    //public static readonly Color OUTLINE_INVALID_TARGET_COLOR = Color.gray;

    #endregion

    #region Editor vars
    [SerializeField]
    private TextMeshProUGUI labelPrefab;

    [SerializeField]
    private ColorPaletteSO colorPalette;
    #endregion

    #region State vars

    private Color baseColor;
    private Color unHoveredColor;
    private Color currentColor;

    private Color CurrentColor
    {
        get { return this.currentColor; }
        set
        {
            this.currentColor = value;
            this.sprite.color = value;
        }
    }
    private TextMeshProUGUI coordLabelTextMesh;
    private TextMeshProUGUI labelTextMesh;
    private string labelString;
    public string LabelString
    {
        get { return labelString; }
        set
        {
            labelString = value;
            this.labelTextMesh.text = value;
        }
    }

    #endregion

    #region Pointers
    private SpriteRenderer sprite;
    private SpriteRenderer outline1;
    private SpriteRenderer outline2;

    #endregion


    #region Startup

    public void Init(bool isStartingZone, bool isLocalStartingZone, HexCoordinates coordinates, SpriteRenderer mainSprite, SpriteRenderer outline1, SpriteRenderer outline2)
    {

        //Debug.LogFormat("Creating hex on client {0} {1} {2}",this.coordinates, this.coordinates.X, this.coordinates.Y);
        this.sprite = mainSprite;
        this.outline1 = outline1;
        this.outline2 = outline2;

        //labels to display single number during navigation (range, etc)
        TextMeshProUGUI numLabel = Instantiate<TextMeshProUGUI>(labelPrefab);
        numLabel.fontSize = 4;
        numLabel.rectTransform.SetParent(LabelCanvas.instance.transform, false);
        numLabel.rectTransform.anchoredPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        this.labelTextMesh = numLabel;

        //coordinates hidden by default using canvas group alpha
        //use that component in editor mode to display
        TextMeshProUGUI coordLabel = Instantiate<TextMeshProUGUI>(labelPrefab);
        coordLabel.rectTransform.SetParent(CoordCanvas.instance.transform, false);
        coordLabel.rectTransform.anchoredPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        coordLabel.text = coordinates.X + " " + coordinates.Y;
        this.coordLabelTextMesh = coordLabel;

        if (isStartingZone && isLocalStartingZone)
        {
            this.baseColor = this.colorPalette.HEX_START_COLOR;
        }
        else if (isStartingZone && !isLocalStartingZone)
        {
            this.baseColor = this.colorPalette.HEX_OPPONENT_START_COLOR;
        }
        else
        {
            this.baseColor = this.colorPalette.HEX_DEFAULT_COLOR;
        }

        this.CurrentColor = this.baseColor;
        this.unHoveredColor = this.baseColor;
    }

    #endregion

    #region State

    public void ShowLabel()
    {
        this.labelTextMesh.alpha = 1;
    }

    public void HideLabel()
    {
        this.labelTextMesh.alpha = 0;
    }

    public void SetOutline1(bool state, Color color)
    {

        this.outline1.color = Utility.SetAlpha(color, state ? 1 : 0);
    }

    public void SetOutline2(bool state, Color color)
    {
        this.outline2.color = Utility.SetAlpha(color, state ? 1 : 0);
    }

    public void Select(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_SELECT_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_SELECT_COLOR : this.baseColor;
    }

    public void DefaultHover(bool mode, bool useOutline = false, bool isMove = false, bool isValidTarget = false)
    {
        if (mode) { 
            this.unHoveredColor = this.CurrentColor;
            this.CurrentColor = this.colorPalette.HEX_HOVER_COLOR;
        } else
        {
            this.CurrentColor = this.unHoveredColor;
        }       

        if (useOutline && !isMove)
            this.SetOutline2(mode, isValidTarget ? this.colorPalette.OUTLINE_VALID_TARGET_COLOR : this.colorPalette.OUTLINE_INVALID_TARGET_COLOR);
        else if (useOutline && isMove)
            this.SetOutline1(mode, isValidTarget ? this.colorPalette.OUTLINE_VALID_TARGET_COLOR : this.colorPalette.OUTLINE_INVALID_TARGET_COLOR);
    }

    public void MoveHover(bool mode, bool isValidTarget = true)
    {
        if (mode) { 
            this.unHoveredColor = this.CurrentColor;
            this.CurrentColor = this.colorPalette.HEX_HOVER_COLOR;
        } else
            this.CurrentColor = this.unHoveredColor;

        this.SetOutline1(mode, isValidTarget ? this.colorPalette.OUTLINE_VALID_TARGET_COLOR : this.colorPalette.OUTLINE_INVALID_TARGET_COLOR);
    }

    public void DisplayInMoveRange(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_IN_MOVE_RANGE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_IN_MOVE_RANGE_COLOR : this.baseColor;
    }

    public void DisplayInAttackRange(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_IN_ATTACK_RANGE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_IN_ATTACK_RANGE_COLOR : this.baseColor;
    }

    public void DisplayInAbilityRange(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_IN_ABILITY_RANGE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_IN_ABILITY_RANGE_COLOR : this.baseColor;
    }

    public void DisplayAttackTargetable(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_ATTACK_TARGETABLE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_ATTACK_TARGETABLE_COLOR : this.baseColor;
    }

    public void DisplayAbilityTargetable(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_ABILITY_TARGETABLE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_ABILITY_TARGETABLE_COLOR : this.baseColor;
    }

    internal void DisplayOutOfActionRange(bool mode)
    {
        this.unHoveredColor = mode ? this.colorPalette.HEX_OUT_OF_ACTION_RANGE_COLOR : this.baseColor;
        this.CurrentColor = mode ? this.colorPalette.HEX_OUT_OF_ACTION_RANGE_COLOR : this.baseColor;
    }

    public void AttackHover(bool mode)
    {
        if (mode) { this.unHoveredColor = this.CurrentColor; }
        this.CurrentColor = mode ? this.colorPalette.HEX_ATTACK_HOVER_COLOR : this.unHoveredColor;
    }
    #endregion

    internal void Delete()
    {
        if (this.coordLabelTextMesh != null)
        {
            Destroy(this.coordLabelTextMesh.gameObject);
        }

        if (this.labelTextMesh != null)
        {
            Destroy(this.labelTextMesh.gameObject);
        }
    }

    public void ClearStartZone()
    {
        this.CurrentColor = this.colorPalette.HEX_DEFAULT_COLOR;
        this.baseColor = this.colorPalette.HEX_DEFAULT_COLOR;
        this.unHoveredColor = this.colorPalette.HEX_DEFAULT_COLOR;
    }
}
