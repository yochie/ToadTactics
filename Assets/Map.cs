using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    //sets orientation of hexes
    public bool isFlatTop = true;

    //radius in hex count
    public int xSize = 50;
    public int ySize = 50;

    public Hex hexPrefab;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite size
    public float hexWidth = 1f;

    // flat to flat, or height, calculated below by constant and width
    private float hexHeight;

    public float padding = 0.1f;

    public Hex[,] hexGrid;

    // Start is called before the first frame update
    void Start()
    {

        hexGrid = new Hex[xSize*2-1, ySize*2-1];
        hexHeight = hexWidth / 1.155f;

        //not sure about this approach, but it works for now..
        float paddedHexWidth = hexWidth + padding;
        float paddedHexHeight = hexHeight + padding;
        

        for (int x = -xSize + 1; x < xSize; x++)
        {
            for (int y = -ySize + 1; y < ySize; y++)
            {
                float xPos;
                if (isFlatTop)
                {
                    xPos =  x * (3f * paddedHexWidth / 4.0f);
                } else {
                    xPos = y % 2 == 0 ? x * paddedHexHeight : x * paddedHexHeight + paddedHexHeight / 2f;
                }

                float yPos;
                if (isFlatTop)
                {
                    yPos = x % 2 == 0 ? y * paddedHexHeight : y * paddedHexHeight + paddedHexHeight / 2f;
                }
                else
                {
                    yPos = y * (3f * paddedHexWidth / 4.0f);
                }

                Hex hex = (Hex)Instantiate(hexPrefab, new Vector3(xPos, yPos, 0), isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0,0,1)));
                if (isFlatTop)
                {
                    hex.transform.localScale = new Vector3(hexWidth, hexWidth, 1);
                } else
                {
                    hex.transform.localScale = new Vector3(hexWidth, hexWidth, 1);
                }
                hex.name = "Hex_" + x + "_" + y;
                hex.transform.SetParent(this.transform);
                hexGrid[x + xSize - 1, y + ySize - 1] = hex;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Hex GetHex(int x, int y)
    {
        return hexGrid[x + this.xSize - 1, y + this.ySize - 1];
    }

    public void SetHex(int x, int y, Hex h)
    {
        hexGrid[x + this.xSize - 1, y + this.ySize - 1] = h;
    }
}
