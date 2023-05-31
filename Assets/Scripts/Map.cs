using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    //sets orientation of hexes
    public bool isFlatTop;

    //radius in hex count
    public int xSize;
    public int ySize;

    public Hex hexPrefab;
    public MapOutline outline;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite width
    public float hexWidth = 1f;

    // flat to flat, or height, calculated on init by WidthToHeightRatio
    private float hexHeight;

    //geometric property of hexes
    private const float WIDTH_TO_HEIGHT_RATIO = 1.155f;

    public float padding = 0.1f;

    private Hex[,] hexGrid;

    private Hex selectedHex;

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

                hex.Init(this, HexCoordinates.FromOffsetCoordinates(x,y), this.transform, "Hex_" + x + "_" + y);

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

    public void SelectHex(Hex h)
    {
        if (this.selectedHex != null)
        {
            UnselectHex();
        }
        this.selectedHex = h;
        h.HexColor = MyUtility.hexSelectedColor;
    }

    public void UnselectHex()
    {
        this.selectedHex.HexColor = MyUtility.hexBaseColor;
        this.selectedHex = null;
    }
}
