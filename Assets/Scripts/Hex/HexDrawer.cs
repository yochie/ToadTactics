using UnityEngine;
using TMPro;

public class HexDrawer : MonoBehaviour
{
    #region Editor vars
    [SerializeField]
    private TextMeshProUGUI labelPrefab;
    #endregion

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

    #region State vars

    private SpriteRenderer sprite;

    private Color baseColor = HexDrawer.HEX_DEFAULT_COLOR;
    private Color unHoveredColor = HexDrawer.HEX_DEFAULT_COLOR;
    private Color hexColor = HexDrawer.HEX_DEFAULT_COLOR;

    private Color currentColor
    {
        get { return this.hexColor; }
        set
        {
            this.hexColor = value;
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

    #region Startup

    public void Init(bool isStartingZone, bool isLocalStartingZone, HexCoordinates coordinates)
    {

        //Debug.LogFormat("Creating hex on client {0} {1} {2}",this.coordinates, this.coordinates.X, this.coordinates.Y);
        this.sprite = this.GetComponent<SpriteRenderer>();

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
            this.baseColor = HexDrawer.HEX_START_COLOR;
        }
        else if (isStartingZone && !isLocalStartingZone)
        {
            this.baseColor = HexDrawer.HEX_OPPONENT_START_COLOR;
        } else
        {
            this.baseColor = HexDrawer.HEX_DEFAULT_COLOR;
        }

        this.currentColor = this.baseColor;
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

    public void Select(bool mode)
    {
        this.unHoveredColor = mode ? HexDrawer.HEX_SELECT_COLOR : this.baseColor;
        this.currentColor = mode ? HexDrawer.HEX_SELECT_COLOR : this.baseColor;
    }

    public void MoveHover(bool mode)
    {
        if (mode) { this.unHoveredColor = this.currentColor; }
        this.currentColor = mode ? HexDrawer.HEX_HOVER_COLOR : this.unHoveredColor;
    }

    public void DisplayMoveRange(bool mode)
    {
        this.unHoveredColor = mode ? HexDrawer.HEX_RANGE_COLOR : this.baseColor;
        this.currentColor = mode ? HexDrawer.HEX_RANGE_COLOR : this.baseColor;
    }
    public void DisplayAttackRange(bool mode)
    {
        this.unHoveredColor = mode ? HexDrawer.HEX_ATTACK_RANGE_COLOR : this.baseColor;
        this.currentColor = mode ? HexDrawer.HEX_ATTACK_RANGE_COLOR : this.baseColor;
    }

    public void DisplayLOSObstruction(bool mode)
    {
        this.unHoveredColor = mode ? HexDrawer.HEX_LOS_OBSTRUCT_COLOR : this.baseColor;
        this.currentColor = mode ? HexDrawer.HEX_LOS_OBSTRUCT_COLOR : this.baseColor;
    }

    internal void DisplayOutOfAttackRange(bool mode)
    {
        this.unHoveredColor = mode ? HexDrawer.HEX_OUT_OF_ATTACK_RANGE_COLOR : this.baseColor;
        this.currentColor = mode ? HexDrawer.HEX_OUT_OF_ATTACK_RANGE_COLOR : this.baseColor;
    }

    public void AttackHover(bool mode)
    {
        if (mode) { this.unHoveredColor = this.currentColor; }
        this.currentColor = mode ? HexDrawer.HEX_ATTACK_HOVER_COLOR : this.unHoveredColor;
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

}
