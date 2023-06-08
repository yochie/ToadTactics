using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class Hex : NetworkBehaviour
{
    public HexCoordinates Coordinates { get; private set; }

    private Color hexColor;
    public Color HexColor {
        get { return this.hexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
        }
    }

    public Color BaseColor { get; set; }

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
    public bool IsStartingZone { get; set; }
    public PlayerCharacter HoldsCharacter { get; set; }
    public Obstacle HoldsObstacle { get; set; }
    public Hazard HoldsHazard { get; set; }
    public bool holdsTreasure { get; set; }

    private Map map;
    private SpriteRenderer sprite;

    public void Init(Map m, HexCoordinates hc, string name, Vector3 position, Vector3 scale, Quaternion rotation) {
        this.sprite = this.GetComponent<SpriteRenderer>();
        this.map = m;
        this.Coordinates = hc;
        this.name = name;
        //this.transform.SetParent(parent);
        this.BaseColor = map.HEX_BASE_COLOR;
        this.IsStartingZone = false;
        this.transform.position = position;
        this.transform.localScale = scale;
        this.transform.rotation = rotation;

        //coordinates hidden by default using canvas group alpha
        //use that component in editor mode to display
        TextMeshProUGUI coordLabel = Instantiate<TextMeshProUGUI>(this.map.cellLabelPrefab);
        coordLabel.rectTransform.SetParent(this.map.coordCanvas.transform, false);
        coordLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        coordLabel.text = this.Coordinates.ToStringOnLines();
        this.coordLabelTextMesh = coordLabel;


        //labels to display single number during navigation (range, etc)
        TextMeshProUGUI numLabel = Instantiate<TextMeshProUGUI>(map.cellLabelPrefab);
        numLabel.fontSize = 4;
        numLabel.rectTransform.SetParent(map.labelsCanvas.transform, false);
        numLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        this.labelTextMesh = numLabel;
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
        this.labelTextMesh.alpha = 1;
    }

    internal void HideLabel()
    {
        this.labelTextMesh.alpha = 0;
    }

    public void DeleteHex()
    {
        Destroy(this.coordLabelTextMesh.gameObject);
        Destroy(this.labelTextMesh.gameObject);
        Destroy(this.gameObject);
    }
}
