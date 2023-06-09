using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class Map : NetworkBehaviour
{
    public static Vector3 characterOffsetOnMap = new(0, 0, -0.1f);

    //sets orientation of hexes
    public bool isFlatTop;

    //radius in hex count
    public int xSize;
    public int ySize;

    public GameObject hexPrefab;
    public MapOutline outline;
    public TextMeshProUGUI cellLabelPrefab;
    public Canvas coordCanvas;
    public Canvas labelsCanvas;
    public List<StartZone> startingZones;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite width
    public float hexWidth = 1f;

    // flat to flat, or height, calculated on init by WIDTH_TO_HEIGHT_RATIO
    private float hexHeight;

    //geometric property of hexes
    private const float WIDTH_TO_HEIGHT_RATIO = 1.155f;
    public float padding = 0.1f;

    public readonly Color HEX_BASE_COLOR = Color.white;
    public readonly Color HEX_START_BASE_COLOR = Color.blue;
    public readonly Color HEX_HOVER_COLOR = Color.cyan;
    public readonly Color HEX_SELECT_COLOR = Color.green;
    public readonly Color HEX_OPPONENT_START_BASE_COLOR = Color.grey;


    private Hex[,] hexGrid;

    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }
    private Dictionary<PlayerCharacter, Hex> characterPositions;

    public void Initialize()
    {
        this.hexHeight = this.hexWidth / WIDTH_TO_HEIGHT_RATIO;

        //only runs on server
        if (isServer)
        {
            this.hexGrid = new Hex[(this.xSize * 2) - 1, (this.ySize * 2) - 1];

            this.GenerateHexes();

            this.characterPositions = new();
        }
    }

    [Server]
    private void GenerateHexes()
    {
        //let server handle this, clients will get results using RPC
        if (!isServer)
        {
            Debug.Log("Skipping map init");
            return;
        }

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

                Vector3 position = new(xPos, yPos, 0);

                Vector3 scale = new(this.hexWidth, this.hexWidth, 1);

                //only rotate if not FlatTop since sprite is by default
                Quaternion rotation = isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0, 0, 1));

                HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(x, y, isFlatTop);

                GameObject hex = Instantiate(this.hexPrefab, position, rotation);
                Hex h = hex.GetComponent<Hex>();
                h.Init(this, coordinates, "Hex_" + x + "_" + y, position, scale, rotation);

                this.SetHex(x, y, h);
                //this.RpcPlaceHex(hex, coordinates, position, scale, rotation, x, y);
            }
        }

        this.outline.DeleteHexesOutside();

        for (int i = 0; i < this.startingZones.Count; i++)
        {
            this.startingZones[i].SetStartingZone();
        }


        //spawn all hexes now that weve cleaned up extras and set all initial state
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                Hex h = GetHex(x, y);
                if(h != null)
                {
                    NetworkServer.Spawn(h.gameObject);
                }                
            }
        }

    }

    //[ClientRpc]
    //public void RpcPlaceHex(GameObject hex, HexCoordinates coordinates, Vector3 position, Vector3 scale, Quaternion rotation, int x, int y)
    //{
    //    if (hex == null) { return; }
    //    Hex h = hex.GetComponent<Hex>();
    //    h.transform.position = position;
    //    h.transform.localScale = scale;
    //    h.transform.rotation = rotation;
    //}

    public Hex GetHex(int x, int y)
    {
        return hexGrid[x + this.xSize - 1, y + this.ySize - 1];
    }

    private void SetHex(int x, int y, Hex h)
    {
        hexGrid[x + this.xSize - 1, y + this.ySize - 1] = h;
    }

    public void DeleteHex(int x, int y)
    {
        Hex toDelete = GetHex(x,y);
        toDelete.DeleteHex();
        hexGrid[x + this.xSize - 1, y + this.ySize - 1] = null;
    }

    public void ClickHex(Hex h)
    {
        //moves previously selected player character
        if (this.SelectedHex != null && this.SelectedHex.holdsCharacter != null)
        {
            this.MovePlayerChar(this.SelectedHex, h);
            this.UnselectHex();
            this.UnhoverHex(h);
            return;
        }

        if (this.SelectedHex != h)
        {
            this.SelectHex(h);
        }
        else
        {
            this.UnselectHex();
        }
    }

    public void MovePlayerChar(Hex source, Hex dest)
    {
        PlayerCharacter toMove = source.holdsCharacter;
        source.holdsCharacter = null;

        this.PlacePlayerChar(toMove, dest);

        this.characterPositions[toMove] = dest;
    }

    public void SelectHex(Hex h)
    {
        if (this.SelectedHex != null)
        {
            UnselectHex();
        }
        this.SelectedHex = h;
        h.HexColor = this.HEX_SELECT_COLOR;
        h.HideLabel();
    }

    public void UnselectHex()
    {
        this.SelectedHex.HexColor = this.SelectedHex.baseColor;
        Hex previouslySelected = this.SelectedHex;
        this.SelectedHex = null;
        this.UnhoverHex(previouslySelected);
        
    }

    public void HoverHex(Hex h)
    {
        this.HoveredHex = h;
        h.HexColor = this.HEX_HOVER_COLOR;
        if (this.SelectedHex != null)
        {
            h.LabelString = Map.HexDistance(this.SelectedHex, this.HoveredHex).ToString();
            h.ShowLabel();
        }
    }
    public void UnhoverHex(Hex h)
    {
        if (this.HoveredHex == h)
        {
            this.HoveredHex = null;
        }

        if (h != this.SelectedHex)
        {
            h.HexColor = h.baseColor;
        }
        else
        {
            h.HexColor = this.HEX_SELECT_COLOR;
        }

        h.HideLabel();
    }

    public static float HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f;
    }

    public void PlacePlayerChar(PlayerCharacter playerChar, Hex destination)
    {
        destination.holdsCharacter = playerChar;
        this.characterPositions[playerChar] = destination;

        //pc.transform.SetParent(position.transform, false);
        playerChar.transform.position = destination.transform.position + characterOffsetOnMap;
        //pc.transform.localPosition = new Vector3(0, 0, -0.1f);
    }
}
