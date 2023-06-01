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
        this.map.hoverHex(this);
    }

    private void OnMouseExit()
    {
        this.map.unhoverHex(this);
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
