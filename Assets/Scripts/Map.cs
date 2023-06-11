using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Utils;

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

    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }

    public readonly Dictionary<Vector2Int, Hex> hexGrid = new();
    public readonly SyncDictionary<Vector2Int, uint> hexGridNetIds = new SyncDictionary<Vector2Int, uint>();

    //TODO : fix using same strat as hexgrid
    //public readonly SyncDictionary<PlayerCharacter, Hex> characterPositions = new();

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
                Quaternion rotation = this.isFlatTop ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0, 0, 1));

                HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(x, y, isFlatTop);

                GameObject hex = Instantiate(this.hexPrefab, position, rotation);
                Hex h = hex.GetComponent<Hex>();
                h.Init(this, coordinates, "Hex_" + x + "_" + y, position, scale, rotation);


                //TODO : Fix grid syncing so that client has access to latest version
                //currently bugged on clients since Hex doesn't exist over there, should probably use strat showes in docs to sync using netid

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
                if (h != null)
                {
                    NetworkServer.Spawn(h.gameObject);
                    this.hexGridNetIds[new Vector2Int(x, y)] = h.gameObject.GetComponent<NetworkIdentity>().netId;
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
                if (h != null && !h.isStartingZone && !h.holdsTreasure && h.holdsHazard.type == HazardType.none)
                {
                    if (UnityEngine.Random.Range(1, 100) <= this.obstacleSpawnChance)
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
        if (this.hexGrid.TryGetValue(new Vector2Int(x, y), out Hex toReturn))
        {
            return toReturn;
        } else
        {
            return null;
        }        
    }

    //public Hex GetCubeHex(int q, int r)
    //{
    //    HexCoordinates toConvert = new(q, r, this.isFlatTop);
    //    return hexGrid[(toConvert.X, toConvert.Y)];
    //}

    [Server]
    private void SetHex(int x, int y, Hex h)
    {
        this.hexGrid[new Vector2Int(x, y)] = h;
    }

    [Server]
    public void DeleteHex(int x, int y)
    {
        Hex toDelete = GetHex(x, y);
        toDelete.DeleteHex();
        this.hexGrid[new Vector2Int(x, y)] = null;
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

    public void HoverHex(Hex hoveredHex)
    {
        this.HoveredHex = hoveredHex;
        hoveredHex.HexColor = this.HEX_HOVER_COLOR;
        if (this.SelectedHex != null)
        {
            Debug.Log(this.HoveredHex.coordinates);

            hoveredHex.LabelString = Map.HexDistance(this.SelectedHex, this.HoveredHex).ToString();
            hoveredHex.ShowLabel();

            this.clearPaths();

            List<Hex> path = this.FindMovementPath(this.SelectedHex, hoveredHex);
            if (path != null)
            {
                this.DisplayPath(path);
            }
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

        this.clearPaths();
    }

    private void DisplayPath(List<Hex> path)
    {
        //Debug.Log("Found path");
        //Debug.Log(path.Count);
        int pathLength = 1;
        foreach (Hex h in path)
        {
            h.LabelString = pathLength.ToString();
            h.ShowLabel();
            pathLength++;
        }
    }

    private void clearPaths()
    {
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                Hex h = GetHex(x, y);
                if (h != null)
                {
                    h.HideLabel();
                }

            }
        }
    }

    public static int HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (int)((Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f);
    }

    public List<Hex> GetHexNeighbours(Hex h)
    {
        List<Hex> toReturn = new();
        foreach (HexCoordinates neighbourCoord in h.coordinates.Neighbours())
        {
            Hex neighbour = GetHex(neighbourCoord.X, neighbourCoord.Y);
            if (neighbour != null)
            {
                toReturn.Add(neighbour);
            }
        }

        return toReturn;
    }

    public List<Hex> GetUnobstructedHexNeighbours(Hex h)
    {
        List<Hex> toReturn = new();
        foreach (HexCoordinates neighbourCoord in h.coordinates.Neighbours())
        {
            Hex neighbour = GetHex(neighbourCoord.X, neighbourCoord.Y);
            if (neighbour != null && !neighbour.holdsObstacle && !neighbour.holdsCharacter)
            {
                toReturn.Add(neighbour);
            }
        }

        return toReturn;
    }

    public List<Hex> FindMovementPath(Hex start, Hex dest)
    {
        PriorityQueue<Hex, int> frontier = new();
        frontier.Enqueue(start, 0);

        Dictionary<Hex, Hex> cameFrom = new();
        cameFrom[start] = null;

        Dictionary<Hex, int> costsSoFar = new();
        costsSoFar[start] = 0;

        while (frontier.Count != 0)
        {
            Hex currentHex = frontier.Dequeue();

            if (currentHex == dest)
            {
                break;
            }

            foreach (Hex next in this.GetUnobstructedHexNeighbours(currentHex))
            {
                //Debug.Log(costsSoFar);
                //Debug.Log(currentHex);
                //Debug.Log(costsSoFar[currentHex]);
                //Debug.Log(next.moveCost);
                int newCost = costsSoFar[currentHex] + next.moveCost;
                if (!costsSoFar.ContainsKey(next) || newCost < costsSoFar[next])
                {
                    //Debug.Log("Adding hex to path");
                    costsSoFar[next] = newCost;
                    int priority = newCost + Map.HexDistance(next, dest);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = currentHex;
                }
            }
        }
        List<Hex> toReturn = this.FlattenPath(cameFrom, dest);
        return toReturn;
    }

    private List<Hex> FlattenPath(Dictionary<Hex, Hex> path, Hex dest)
    {
        //no path to destination was found
        if (!path.ContainsKey(dest))
        {
            return null;
        }

        List<Hex> toReturn = new();
        Hex currentHex = dest;
        while (path[currentHex] != null)
        {
            toReturn.Add(currentHex);
            currentHex = path[currentHex];
        }
        toReturn.Reverse();
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

        //this.characterPositions[toMove] = dest;

        this.RpcPlaceChar(toMove.gameObject, dest.transform.position);

    }

    [ClientRpc]
    public void RpcPlaceChar(GameObject character, Vector3 position)
    {
        character.transform.position = position + Map.characterOffsetOnMap;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        hexGridNetIds.Callback += OnHexGridNetIdsChange;
        // Process initial SyncDictionary payload
        foreach (KeyValuePair<Vector2Int, uint> kvp in hexGridNetIds)
            OnHexGridNetIdsChange(SyncDictionary<Vector2Int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    [Client]
    void OnHexGridNetIdsChange(SyncDictionary<Vector2Int, uint>.Operation op, Vector2Int key, uint netId)
    {

        switch (op)
        {
            case SyncDictionary<Vector2Int, uint>.Operation.OP_ADD:
                // entry added
                this.hexGrid[key] = null;

                if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity))
                {
                    this.hexGrid[key] = identity.gameObject.GetComponent<Hex>();
                } else
                {
                    StartCoroutine(HexFromNetIdCoroutine(key, netId));
                }
                break;
            case SyncDictionary<Vector2Int, uint>.Operation.OP_SET:
                // entry changed
                break;
            case SyncDictionary<Vector2Int, uint>.Operation.OP_REMOVE:
                // entry removed
                break;
            case SyncDictionary<Vector2Int, uint>.Operation.OP_CLEAR:
                // Dictionary was cleared
                break;
        }
    }

    [Client]
    IEnumerator HexFromNetIdCoroutine(Vector2Int key, uint netId)
    {
        while (this.hexGrid[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity))
                this.hexGrid[key] = identity.gameObject.GetComponent<Hex>();
        }
    }
}