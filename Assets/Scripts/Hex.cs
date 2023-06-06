using System;
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
    private Map map;

    public Color HexColor {
        get { return this.HexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
        }
    }

    public Color BaseColor { get; set; }

    public bool IsStartingZone { get; set; }

    //should only be edited for initial setting of ref, thereafter use LabelString
    //cant put in init because this happens afterwards
    //TODO : find way to enforce this
    public TextMeshProUGUI LabelTextMesh { private get;  set; }
    public string LabelString {
        get { return labelString;  }
        set { 
            labelString = value;
            LabelTextMesh.text = value;
        } 
    }

    public PlayerCharacter HoldsCharacter { get; set; }
    public Obstacle HoldsObstacle { get; set; }
    public Hazard HoldsHazard { get; set; }
    public bool holdsTreasure { get; set; }


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
        this.BaseColor = map.HEX_BASE_COLOR;
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseEnter() {
        this.map.hoverHex(this);
    }

    private void OnMouseExit()
    {
        this.map.unhoverHex(this);
    }

    private void OnMouseDown()
    {
        this.map.clickHex(this);
    }

    internal void ShowLabel()
    {
        this.LabelTextMesh.alpha = 1;
    }

    internal void HideLabel()
    {
        this.LabelTextMesh.alpha = 0;
    }
}
