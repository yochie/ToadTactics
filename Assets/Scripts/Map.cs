using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Map : MonoBehaviour
{
    //sets orientation of hexes
    public bool isFlatTop;

    //radius in hex count
    public int xSize;
    public int ySize;

    public Hex hexPrefab;
    public MapOutline outline;
    public TextMeshProUGUI cellLabelPrefab;
    public Canvas coordCanvas;
    public Canvas labelsCanvas;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite width
    public float hexWidth = 1f;

    // flat to flat, or height, calculated on init by WidthToHeightRatio
    private float hexHeight;

    //geometric property of hexes
    private const float WIDTH_TO_HEIGHT_RATIO = 1.155f;

    public float padding = 0.1f;

    public Color HEX_BASE_COLOR = Color.white;
    public Color HEX_HOVER_COLOR = Color.cyan;
    public Color HEX_SELECT_COLOR = Color.green;

    private Hex[,] hexGrid;

    private Hex selectedHex;
    private Hex hoveredHex;


    //read only, to edit, use SelectHex() or UnselectHex()
    public Hex SelectedHex
    {
        get { return this.selectedHex;  }
    }

    public void Initialize()
    {
        this.hexGrid = new Hex[(this.xSize * 2) - 1, (this.ySize * 2) - 1];
        this.hexHeight = this.hexWidth / WIDTH_TO_HEIGHT_RATIO;
        this.GenerateHexes();
        this.outline.DeleteHexesOutside();
    }

    private void GenerateHexes()
    {
        float paddedHexWidth = this.hexWidth + this.padding;
        float paddedHexHeight = this.hexHeight + this.padding;
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                float xPos;
                if (this.isFlatTop)
                {
                    xPos = x * (3f * paddedHexWidth / 4.0f);
                }
                else
                {
                    xPos = y % 2 == 0 ? x * paddedHexHeight : x * paddedHexHeight + paddedHexHeight / 2f;
                }

                float yPos;
                if (this.isFlatTop)
                {
                    yPos = x % 2 == 0 ? y * paddedHexHeight : y * paddedHexHeight + paddedHexHeight / 2f;
                }
                else
                {
                    yPos = y * (3f * paddedHexWidth / 4.0f);
                }

                //only rotate if not FlatTop since sprite is by default
                Quaternion rotation = isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0, 0, 1));
                Hex hex = (Hex)Instantiate(this.hexPrefab, new Vector3(xPos, yPos, 0), rotation);
                if (isFlatTop)
                {
                    hex.transform.localScale = new Vector3(hexWidth, hexWidth, 1);
                }
                else
                {
                    hex.transform.localScale = new Vector3(hexWidth, hexWidth, 1);
                }

                hex.Init(this, HexCoordinates.FromOffsetCoordinates(x,y, isFlatTop), this.transform, "Hex_" + x + "_" + y);

                //offset by size to balance negative coordinates
                this.hexGrid[x + this.xSize - 1, y + this.ySize - 1] = hex;

            }
        }
    }

    public Hex GetHex(int x, int y)
    {
        return hexGrid[x + this.xSize - 1, y + this.ySize - 1];
    }

    public void SetHex(int x, int y, Hex h)
    {
        hexGrid[x + this.xSize - 1, y + this.ySize - 1] = h;
    }

    public void clickHex(Hex h)
    {
        if (this.SelectedHex != h)
        {
            this.SelectHex(h);
        }
        else
        {
            this.UnselectHex();
        }
    }
    public void SelectHex(Hex h)
    {
        if (this.selectedHex != null)
        {
            UnselectHex();
        }
        this.selectedHex = h;
        h.HexColor = this.HEX_SELECT_COLOR;
        h.LabelTextMesh.alpha = 0;
    }

    public void UnselectHex()
    {
        this.selectedHex.HexColor = this.HEX_BASE_COLOR;
        Hex previouslySelected = this.selectedHex;
        this.selectedHex = null;
        this.unhoverHex(previouslySelected);
        
    }

    public void hoverHex(Hex h)
    {
        this.hoveredHex = h;
        h.HexColor = this.HEX_HOVER_COLOR;
        if (this.SelectedHex != null)
        {
            h.LabelString = Map.HexDistance(this.SelectedHex, this.hoveredHex).ToString();
            h.LabelTextMesh.alpha = 1;
        }
    }
    public void unhoverHex(Hex h)
    {
        if (this.hoveredHex == h)
        {
            this.hoveredHex = null;
        }

        if (h != this.SelectedHex)
        {
            h.HexColor = this.HEX_BASE_COLOR;
        }
        else
        {
            h.HexColor = this.HEX_SELECT_COLOR;
        }

        h.LabelTextMesh.alpha = 0;
    }

    public static float HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f;
    }
}
