using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    //sets orientation of hexes
    public bool isFlatTop = true;

    //cuts off hexes beyond flatSize diameter
    public bool isCircular= true;

    //size of tallest column in flatTop orientation
    public int flatSize = 7;

    public Hex hexPrefab;

    //point to point, or width (two times side length)
    public float hexWidth = 1f;

    // flat to flat, or height, calculated below by constant and width
    private float hexHeight;

    private Hex[,] hexGrid;

    // Start is called before the first frame update
    void Start()
    {

        bool oddFlatSize = flatSize % 2 == 1 ? true : false;

        if (!oddFlatSize)
        {
            Debug.LogError("Only odd flatSize is currently supported");
        }

        if (!isFlatTop)
        {
            Debug.LogError("Only flatTop is currently supported");
        }

        hexGrid = new Hex[flatSize*2,flatSize];
        //adds a bit of padding between hexes
        hexWidth += 0.1f;
        hexHeight = hexWidth / 1.155f;

        int startingX = isCircular ? -(flatSize / 2) : -(flatSize - 1);
        int endingX = -startingX;
        for (int x = startingX; x <= endingX; x++)
        {
            int colSize = flatSize - Mathf.Abs(x);
            int startingY = -(colSize - 1) / 2;
            int endingY = colSize + startingY -1;
            for (int y = startingY; y <= endingY; y++)
            {
                float xPos = (3 * hexWidth / 4) * x;
                float yPos = Mathf.Abs(x) % 2 == 1 ? (y * hexHeight) - (hexHeight / 2) : (y * hexHeight);
                Hex hex = (Hex)Instantiate(hexPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
                hex.name = "Hex_" + x + "_" + y;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
