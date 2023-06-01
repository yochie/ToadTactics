using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hex : MonoBehaviour
{
    private Color hexColor;
    private SpriteRenderer sprite;
    private string labelString;
    public HexCoordinates coordinates;

    public Color HexColor {
        get { return this.HexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
        }
    }

    //should only be edited for initial setting of ref, thereafter use LabelString
    //cant put in init because this happens afterwards
    //TODO : find way to enforce this
    public TextMeshProUGUI LabelTextMesh { get; set; }
    public string LabelString {
        get { return labelString;  }
        set { 
            labelString = value;
            LabelTextMesh.text = value;
        } 
    }

    private Map map;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(Map m, HexCoordinates hc, Transform parent, string name) {
        this.sprite = this.GetComponent<SpriteRenderer>();
        this.map = m;
        this.coordinates = hc;
        this.name = name;
        this.transform.SetParent(parent);
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseEnter() {
        this.HexColor = MyUtility.hexHoverColor;
        this.map.hoveredHex = this;
        if (map.SelectedHex != null) {
            this.LabelString = MyUtility.HexDistance(map.SelectedHex, this).ToString();
            this.LabelTextMesh.alpha = 1;
        }
    }

    private void OnMouseExit()
    {
        if (this.map.SelectedHex != this)
        {
            this.HexColor = MyUtility.hexBaseColor;
        } else
        {
            this.HexColor = MyUtility.hexSelectedColor;
        }

        this.LabelTextMesh.alpha = 0;
    }

    private void OnMouseDown()
    {
        if (this.map.SelectedHex != this)
        {
            this.map.SelectHex(this);
        } else
        {
            this.map.UnselectHex();
        }
    }

}
