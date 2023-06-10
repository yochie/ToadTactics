using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class Map : NetworkBehaviour
{
    public static readonly Vector3 characterOffsetOnMap = new(0, 0, -0.1f);
    //sets orientation of hexes
    public bool isFlatTop;

    //radius in hex count
    public int xSize;
    public int ySize;

    public int obstacleSpawnChance;

    public GameObject hexPrefab;
    public GameObject treePrefab;

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

    private Dictionary<(int,int), Hex> hexGrid = new();

    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }

    public readonly SyncDictionary<PlayerCharacter, Hex> characterPositions = new();

    public void Initialize()
    {
        this.hexHeight = this.hexWidth / WIDTH_TO_HEIGHT_RATIO;

        //only runs on server
        if (isServer)
        {
            this.GenerateHexes();
        }
    }

    [Server]
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

                Vector3 position = new(xPos, yPos, 0);

                Vector3 scale = new(this.hexWidth, this.hexWidth, 1);

                //only rotate if not FlatTop since sprite is by default
                Quaternion rotation = isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0, 0, 1));

                HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(x, y, isFlatTop);

                GameObject hex = Instantiate(this.hexPrefab, position, rotation);
                Hex h = hex.GetComponent<Hex>();
                h.Init(this, coordinates, "Hex_" + x + "_" + y, position, scale, rotation);

                this.SetHex(x, y, h);
            }
        }

        this.outline.DeleteHexesOutside();

        //sets flag on hexes that are starting zones
        //also assigns player
        for (int i = 0; i < this.startingZones.Count; i++)
        {
            this.startingZones[i].SetStartingZone();
        }

        //place random obstacles
        this.GenerateTrees();

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

    [Server]
    private void GenerateTrees()
    {
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                Hex h = GetHex(x, y);
                if (h != null && !h.isStartingZone && !h.holdsTreasure && h.holdsHazard == Hazard.none)
                {
                    if(UnityEngine.Random.Range(1, 100) <= this.obstacleSpawnChance)
                    {
                        Debug.Log("Spawning tree");
                        GameObject tree = Instantiate(this.treePrefab, h.transform.position, Quaternion.identity);
                        NetworkServer.Spawn(tree);
                        h.holdsObstacle = tree.GetComponent<Obstacle>();
                    }

                }
            }
        }
    }

    public Hex GetHex(int x, int y)
    {
        return this.hexGrid[(x, y)];
    }

    //public Hex GetCubeHex(int q, int r)
    //{
    //    HexCoordinates toConvert = new(q, r, this.isFlatTop);
    //    return hexGrid[(toConvert.X, toConvert.Y)];
    //}

    private void SetHex(int x, int y, Hex h)
    {
        hexGrid[(x, y)] = h;
    }

    public void DeleteHex(int x, int y)
    {
        Hex toDelete = GetHex(x,y);
        toDelete.DeleteHex();
        hexGrid[(x, y)] = null;
    }

    public void ClickHex(Hex clickedHex)
    {
        //moves previously selected player character
        if (this.SelectedHex != null && this.SelectedHex.holdsCharacter != null)
        {
            this.CmdMoveChar(this.SelectedHex, clickedHex);
            this.UnselectHex();
            this.UnhoverHex(clickedHex);
            return;
        }

        if (this.SelectedHex != clickedHex)
        {
            this.SelectHex(clickedHex);
        }
        else
        {
            this.UnselectHex();
        }
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

    public Hex[] GetHexNeighbours(Hex h)
    {
        Hex[] toReturn = new Hex[6];
        int i = 0;
        foreach (HexCoordinates neighbourCoord in h.coordinates.Neighbours())
        {
            toReturn[i] =  GetHex(neighbourCoord.X, neighbourCoord.Y);
            i++;
        }

        return toReturn;
    }

    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        //Validation
        //TODO: add pathing
        if (source == null ||
            source.holdsCharacter == null ||
            source.holdsCharacter.netIdentity.connectionToClient != sender ||
            dest.holdsObstacle != null ||
            dest.holdsCharacter != null)
        {
            Debug.Log("Client requested invalid move");
            return;
        }

        PlayerCharacter toMove = source.holdsCharacter;

        source.holdsCharacter = null;

        dest.holdsCharacter = toMove;

        this.characterPositions[toMove] = dest;

        this.RpcPlaceChar(toMove.gameObject, dest.transform.position);

    }

    [ClientRpc]
    public void RpcPlaceChar(GameObject character, Vector3 position)
    {
        character.transform.position = position + Map.characterOffsetOnMap;
    }
}
