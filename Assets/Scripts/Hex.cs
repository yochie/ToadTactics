using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    private Color hexColor;
    private SpriteRenderer sprite;
    private Map map;
    public HexCoordinates coordinates;

    public Color HexColor {
        get { return this.HexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
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
        this.HexColor = MyUtility.hexHoverColor;
        map.hoveredHex = this;
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
