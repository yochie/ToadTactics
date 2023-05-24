using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    //sets orientation of hexes
    public bool isFlatTop = true;

    public int xSize = 50;
    public int ySize = 50;

    public Hex hexPrefab;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite size
    public float hexWidth = 1f;

    // flat to flat, or height, calculated below by constant and width
    private float hexHeight;

    public float padding = 0.1f;

    private Hex[,] hexGrid;

    // Start is called before the first frame update
    void Start()
    {

        hexGrid = new Hex[xSize, ySize];

        //not sure about this approach, but it works for now..
        hexWidth += padding;
        hexHeight = hexWidth / 1.155f;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                float xPos;
                if (isFlatTop)
                {
                    xPos =  x * (3f * hexWidth / 4.0f);
                } else {
                    xPos = y % 2 == 0 ? x * hexHeight : x * hexHeight + hexHeight / 2f;
                }
                xPos += Camera.main.ViewportToWorldPoint(Vector3.zero).x;

                float yPos;
                if (isFlatTop)
                {
                    yPos = x % 2 == 0 ? y * hexHeight : y * hexHeight + hexHeight / 2f;
                }
                else
                {
                    yPos = y * (3f * hexWidth / 4.0f);
                }
                yPos += Camera.main.ViewportToWorldPoint(Vector3.zero).y;

                Hex hex = (Hex)Instantiate(hexPrefab, new Vector3(xPos, yPos, 0), isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0,0,1)));
                hex.name = "Hex_" + x + "_" + y;
                hexGrid[x, y] = hex;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
