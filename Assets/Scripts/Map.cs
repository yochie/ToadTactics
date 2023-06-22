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

    #region Editor vars
    public bool isFlatTop;

    //radius in hex count
    public int xSize;
    public int ySize;

    public int obstacleSpawnChance;

    public GameObject hexPrefab;
    public GameObject treePrefab;

    public MapOutline outline;
    public TreasureSpawner treasureSpawner;

    public TextMeshProUGUI cellLabelPrefab;
    public Canvas coordCanvas;
    public Canvas labelsCanvas;
    public List<StartZone> startingZones;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite width
    public float hexWidth = 1f;
    //flat to flat, or height, calculated on init by WIDTH_TO_HEIGHT_RATIO
    private float hexHeight;

    public float padding = 0.1f;

    public LayerMask hexMask;
    #endregion

    #region Constant vars
    //geometric property of hexes
    private const float WIDTH_TO_HEIGHT_RATIO = 1.155f;
    #endregion

    #region Synced vars
    public readonly Dictionary<Vector2Int, Hex> hexGrid = new();
    public readonly SyncDictionary<Vector2Int, uint> hexGridNetIds = new();

    //TODO : fix using same strat as hexgrid
    //public readonly SyncDictionary<PlayerCharacter, Hex> characterPositions = new();
    #endregion

    #region Runtime state vars
    public static Map Singleton { get; private set; }
    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }
    private HashSet<Hex> displayedRange = new();
    private List<Hex> displayedPath = new();
    public ControlMode controlMode;
    public bool hexesSpawnedOnClient;

    #endregion

    #region Startup
    private void Awake()
    {
        Singleton = this;
        hexesSpawnedOnClient = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.controlMode = ControlMode.move;

        hexGridNetIds.Callback += OnHexGridNetIdsChange;
        // Process initial SyncDictionary payload
        foreach (KeyValuePair<Vector2Int, uint> kvp in hexGridNetIds)
            OnHexGridNetIdsChange(SyncDictionary<Vector2Int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    public void Initialize()
    {
        this.hexHeight = this.hexWidth / WIDTH_TO_HEIGHT_RATIO;

        if (isServer)
        {
            this.GenerateMap();
        }
    }

    #endregion

    #region Generation
    [Server]
    private void GenerateMap()
    {
        this.GenerateHexes();

        this.outline.DeleteHexesOutside();

        //sets flag on hexes that are starting zones
        //also assigns player
        for (int i = 0; i < this.startingZones.Count; i++)
        {
            this.startingZones[i].SetStartingZone();
        }

        this.treasureSpawner.SpawnTreasure();

        this.GenerateTrees();

        //spawn all hexes on clients now that weve cleaned up extras and set all initial state
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                Hex h = GetHex(x, y);
                if (h != null)
                {
                    NetworkServer.Spawn(h.gameObject);

                    //used to sync hexGrid using coroutine callbacks on client
                    //bypasses issues with syncing gameobjects that haven't been spawned yet
                    this.hexGridNetIds[new Vector2Int(x, y)] = h.gameObject.GetComponent<NetworkIdentity>().netId;
                }
            }
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
                h.Init(coordinates, "Hex_" + x + "_" + y, position, scale, rotation);

                this.SetHex(x, y, h);
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
                if (h != null && !h.isStartingZone && !h.holdsTreasure && h.holdsHazard == HazardType.none)
                {
                    if (UnityEngine.Random.Range(1, 100) <= this.obstacleSpawnChance)
                    {
                        //Debug.Log("Spawning tree");
                        GameObject tree = Instantiate(this.treePrefab, h.transform.position, Quaternion.identity);
                        NetworkServer.Spawn(tree);
                        h.holdsObstacle = ObstacleType.tree;
                    }

                }
            }
        }
    }

    [Server]
    private void SetHex(int x, int y, Hex h)
    {
        this.hexGrid[new Vector2Int(x, y)] = h;
    }

    [Server]
    public void DeleteHex(int x, int y)
    {
        Hex toDelete = GetHex(x, y);
        toDelete.Delete();
        this.hexGrid.Remove(new Vector2Int(x, y));
    }
    #endregion

    #region State management
    public Hex GetHex(int x, int y)
    {
        if (this.hexGrid.TryGetValue(new Vector2Int(x, y), out Hex toReturn))
        {
            return toReturn;
        }
        else
        {
            return null;
        }
    }


    public void StartDragHex(Hex draggedHex)
    {
        this.UnselectHex();
        this.SelectHex(draggedHex);
    }

    public void EndDragHex(Hex startHex)
    {
        Hex endHex = this.HoveredHex;               
        this.CmdMoveChar(startHex, endHex);
        this.UnselectHex();
    }

    public void ClickHex(Hex clickedHex)
    {
        Hex previouslySelected = this.SelectedHex;

        if (previouslySelected != null && previouslySelected.HoldsACharacter())
        {
            this.CmdMoveChar(previouslySelected, clickedHex);
            this.UnselectHex();
            return;
        }

        if (previouslySelected != clickedHex)
        {
            this.UnselectHex();
            this.SelectHex(clickedHex);
        }
        else
        {
            this.UnselectHex();
        }
    }

    public void SelectHex(Hex h)
    {
        if (h.HoldsACharacter())
        {
            int heldCharacterID = h.holdsCharacterWithClassID;
            PlayerCharacter heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
            this.DisplayMovementRange(h, heldCharacter.CanMoveDistance());
        }

        this.SelectedHex = h;
        h.Select(true);
    }

    public void UnselectHex()
    {
        Debug.Log("Unselecting hex");
        this.HideMovementRange();
        this.HidePath();
        if (this.SelectedHex == null) { return; }
        this.SelectedHex.Select(false);
        this.SelectedHex = null;        
    }

    public void HoverHex(Hex hoveredHex)
    {
        this.HoveredHex = hoveredHex;
        hoveredHex.MoveHover(true);

        //find path to hex if we have selected another hex
        if (this.SelectedHex != null)
        {
            this.HidePath();
            //hoveredHex.LabelString = Map.HexDistance(this.SelectedHex, this.HoveredHex).ToString();
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
        h.MoveHover(false);

        //h.HideLabel();

        //this.HidePath();
    }

    private void DisplayMovementRange(Hex position, int moveDistance)
    {
        this.displayedRange = RangeWithObstaclesAndCost(position, moveDistance);
        foreach (Hex h in this.displayedRange)
        {
            //selected hex stays at selected color state
            if(h != position)
            {
                h.DisplayRange(true);
            }
        }
    }

    private void HideMovementRange()
    {
        foreach (Hex h in this.displayedRange)
        {
            h.DisplayRange(false);
        }
    }

    private void DisplayPath(List<Hex> path)
    {
        this.displayedPath = path;
        int pathLength = 0;
        foreach (Hex h in path)
        {
            pathLength += h.MoveCost;

            //skip starting hex label
            if (pathLength != 0)
            {
                h.LabelString = pathLength.ToString();
                h.ShowLabel();
            }
        }
    }

    private void HidePath()
    {
        foreach (Hex h in this.displayedPath)
        {
            h.HideLabel();
        }
    }
    #endregion

    #region Pathing
    public static int HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (int)((Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f);
    }

    public List<Hex> RangeIgnoringObstacles(Hex start, int distance)
    {
        List<Hex> toReturn = new();

        for (int q = -distance; q <= distance; q++)
        {
            for (int r = Mathf.Max(-distance, -distance - q); r <= Mathf.Min(distance, -q + distance); r++)
            {
                HexCoordinates destCoords = HexCoordinates.Add(start.coordinates, new HexCoordinates(q, r, start.coordinates.isFlatTop));
                Hex destHex = Map.Singleton.GetHex(destCoords.X, destCoords.Y);
                if (destHex != null)
                    toReturn.Add(destHex);
            }
        }
        //Debug.Log(toReturn);
        //Debug.Log(toReturn.Count);
        return toReturn;
    }

    public HashSet<Hex> RangeWithObstacles(Hex start, int distance)
    {
        HashSet<Hex> visited = new();
        visited.Add(start);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex> { start });

        for (int k = 1; k <= distance; k++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex h in fringes[k - 1])
            {
                foreach (Hex neighbour in Map.Singleton.GetUnobstructedHexNeighbours(h))
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        fringes[k - 1 + neighbour.MoveCost].Add(neighbour);
                    }

                }
            }
        }

        return visited;
    }

    public HashSet<Hex> RangeWithObstaclesAndCost(Hex start, int distance)
    {
        HashSet<Hex> visited = new();
        visited.Add(start);
        List<List<Hex>> fringes = new();
        fringes.Add(new List<Hex> { start });
        Dictionary<Hex, int> costsSoFar = new();
        costsSoFar[start] = 0;

        for (int k = 1; k <= distance; k++)
        {
            fringes.Add(new List<Hex>());
            foreach (Hex h in fringes[k - 1])
            {
                foreach (Hex neighbour in Map.Singleton.GetUnobstructedHexNeighbours(h))
                {
                    int costToNeighbour = costsSoFar[h] + neighbour.MoveCost;
                    if (!costsSoFar.ContainsKey(neighbour) || costsSoFar[neighbour] > costToNeighbour)
                    {
                        if (costToNeighbour <= distance)
                        {
                            costsSoFar[neighbour] = costToNeighbour;
                            visited.Add(neighbour);
                            fringes[k].Add(neighbour);
                        }
                    }

                }
            }
        }

        return visited;
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

    //removes hexes with hazards, obstacles or players
    public List<Hex> GetUnobstructedHexNeighbours(Hex h)
    {
        List<Hex> toReturn = new();
        foreach (HexCoordinates neighbourCoord in h.coordinates.Neighbours())
        {
            Hex neighbour = GetHex(neighbourCoord.X, neighbourCoord.Y);
            if (neighbour != null && neighbour.holdsObstacle == ObstacleType.none && neighbour.holdsCharacterWithClassID == -1)
            {
                toReturn.Add(neighbour);
            }
        }

        return toReturn;
    }

    private int PathCost(List<Hex> path)
    {
        int pathCost = 0;

        foreach(Hex h in path)
        {
            pathCost += h.MoveCost;
        }

        return pathCost;
    }

    private List<Hex> FindMovementPath(Hex start, Hex dest)
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
                int newCost = costsSoFar[currentHex] + next.MoveCost;
                if (!costsSoFar.ContainsKey(next) || newCost < costsSoFar[next])
                {
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

    private  List<Hex> FlattenPath(Dictionary<Hex, Hex> path, Hex dest)
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

    public bool LOSReaches(Hex source, Hex dest)
    {
        RaycastHit2D[] hits;
        Vector2 sourcePos = source.transform.position;
        Vector2 destPos = dest.transform.position;
        Vector2 direction = destPos - sourcePos;

        hits = Physics2D.RaycastAll(sourcePos, direction, 999, hexMask);
        foreach(RaycastHit2D hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            Hex hitHex = hitObject.GetComponent<Hex>();
            if (hitHex != null)
            {
                hitHex.DisplayLOSObstruction(true);
            }
        }
        return false;
    }
    #endregion

    #region Commands
    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        PlayerController senderPlayer = sender.identity.gameObject.GetComponent<PlayerController>();        
        //Validation
        if (dest == null ||
            source == null ||
            source == dest ||
            !source.HoldsACharacter() ||
            !GameController.Singleton.DoesHeOwnThisCharacter(senderPlayer.playerIndex, source.holdsCharacterWithClassID) ||
            dest.holdsObstacle != ObstacleType.none ||
            dest.HoldsACharacter()        
            )
        {
            Debug.Log("Client requested invalid move");
            return;
        }
        PlayerCharacter toMove = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
        int moveCost = PathCost(FindMovementPath(source, dest));
        if (moveCost > toMove.CanMoveDistance()) {
            Debug.Log("Client requested move outside his current range");
            return;
        }
        toMove.UseMoves(moveCost);
        dest.holdsCharacterWithClassID = source.holdsCharacterWithClassID;
        source.clearCharacter();
        this.RpcPlaceChar(toMove.gameObject, dest.transform.position);

    }

    [Command(requiresAuthority = false)]
    public void CmdCreateCharOnBoard(int characterClassID, Hex destinationHex, NetworkConnectionToClient sender = null)
    {
        int ownerPlayerIndex = sender.identity.gameObject.GetComponent<PlayerController>().playerIndex;
        //validate destination
        if (destinationHex == null ||
            !destinationHex.isStartingZone ||
            destinationHex.startZoneForPlayerIndex != ownerPlayerIndex ||
            destinationHex.holdsCharacterWithClassID != -1)
        {
            Debug.Log("Invalid character destination");
            return;
        }

        GameObject characterPrefab = GameController.Singleton.GetCharPrefabWithClassID(characterClassID);
        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newChar =
            Instantiate(characterPrefab, destinationWorldPos, Quaternion.identity);
        NetworkServer.Spawn(newChar, connectionToClient);
        GameController.Singleton.playerCharactersNetIDs.Add(characterClassID, newChar.GetComponent<NetworkIdentity>().netId);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = characterClassID;

        Map.Singleton.RpcPlaceChar(newChar, destinationWorldPos);
        this.markCharacterSlotAsPlaced(sender, characterClassID);

        GameController.Singleton.EndTurn();
    }
    #endregion

    #region RPCs
    //update client UI to prevent placing same character twice
    [TargetRpc]
    public void markCharacterSlotAsPlaced(NetworkConnectionToClient target, int classID)
    {
        foreach (CharacterSlotUI slot in GameController.Singleton.characterSlots)
        {
            if (slot.HoldsCharacterWithClassID == classID)
            {
                slot.HasBeenPlacedOnBoard = true;
            }
        }
    }

    //update all clients UI to display character
    [ClientRpc]
    public void RpcPlaceChar(GameObject character, Vector3 position)
    {
        character.transform.position = position;
    }

    [ClientRpc]
    public void RpcClearState()
    {
        this.UnselectHex();
        this.HidePath();
        this.HideMovementRange();        
    }


    //callback for syncing hex grid dict netids
    [Client]
    void OnHexGridNetIdsChange(SyncDictionary<Vector2Int, uint>.Operation op, Vector2Int key, uint netIdArg)
    {

        switch (op)
        {
            case SyncDictionary<Vector2Int, uint>.Operation.OP_ADD:
                // entry added
                this.hexGrid[key] = null;

                if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
                {
                    this.hexGrid[key] = identity.gameObject.GetComponent<Hex>();
                }
                else
                {
                    StartCoroutine(HexFromNetIdCoroutine(key, netIdArg));
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

    //coroutine to finish matching netids
    [Client]
    IEnumerator HexFromNetIdCoroutine(Vector2Int key, uint netIdArg)
    {
        while (this.hexGrid[key] == null)
        {
            yield return null;
            if (NetworkClient.spawned.TryGetValue(netIdArg, out NetworkIdentity identity))
                this.hexGrid[key] = identity.gameObject.GetComponent<Hex>();
        }
    }
    #endregion

    public void Update()
    {
        if (isServer && this.hexGrid != null)
        {
            foreach (Hex h in this.hexGrid.Values)
            {
                if (h == null || !h.hasBeenSpawnedOnClient)
                {
                    //client isn't ready
                    return;
                }
            }
            this.hexesSpawnedOnClient = true;
        }
    }
}