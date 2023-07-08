using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Utils;
using System.Linq;


//RTODO: Split into seperate classes
//RTODO: Commands made into actions/ moved to other classes
[RequireComponent(typeof(MapGenerator))]
public class Map : NetworkBehaviour
{

    #region Editor vars
    [SerializeField]
    private LayerMask hexMask;
    [SerializeField]
    private MapGenerator mapGenerator;
    #endregion

    #region Synced vars
    public readonly Dictionary<Vector2Int, Hex> hexGrid = new();
    public readonly SyncDictionary<Vector2Int, uint> hexGridNetIDs = new();

    //maps classID onto HexCoordinates
    public readonly SyncDictionary<int, HexCoordinates> characterPositions = new();
    #endregion

    #region Runtime state vars
    public static Map Singleton { get; private set; }
    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }
    private HashSet<Hex> displayedMoveRange = new();
    private Dictionary<Hex,LOSTargetType> displayedAttackRange = new();

    private List<Hex> displayedPath = new();
    private ControlMode currentControlMode;
    public ControlMode CurrentControlMode {
        get { return this.currentControlMode; }
        set { 
            this.currentControlMode = value;
            GameController.Singleton.HighlightGameplayButton(value);
        }

    
    }

    [HideInInspector]
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

        this.CurrentControlMode = ControlMode.move;

        hexGridNetIDs.Callback += OnHexGridNetIdsChange;
        // Process initial SyncDictionary payload
        foreach (KeyValuePair<Vector2Int, uint> kvp in hexGridNetIDs)
            OnHexGridNetIdsChange(SyncDictionary<Vector2Int, uint>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    [Server]
    public void Initialize()
    {
        Dictionary<Vector2Int, uint> generatedHexNetIds = this.mapGenerator.GenerateMap();
        foreach (KeyValuePair<Vector2Int, uint> entry in generatedHexNetIds)
        {
            this.hexGridNetIDs.Add(entry.Key, entry.Value);
        }
    }

    #endregion

    #region State management
    //only used for movement
    public void StartDragHex(Hex draggedHex)
    {
        if (draggedHex == null || !draggedHex.IsValidMoveSource())
        {
            this.UnselectHex();
        } else
        {
            this.UnselectHex();
            this.SelectHex(draggedHex);
        }
    }

    //only used for movement
    public void EndDragHex(Hex startHex)
    {

        Hex endHex = this.HoveredHex;

        if (endHex == null || !this.IsValidMoveForPlayer(GameController.Singleton.LocalPlayer.playerID, startHex, endHex))
        {
            this.UnselectHex();
            return;
        }

        this.CmdMoveChar(startHex, endHex);
        this.UnselectHex();
    }

    public void ClickHex(Hex clickedHex)
    {
        Hex previouslySelected = this.SelectedHex;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:

                if (previouslySelected == null)
                {
                    //FIRST CLICK
                    this.SelectHex(clickedHex);
                }
                else if (this.IsValidMoveForPlayer(GameController.Singleton.LocalPlayer.playerID, previouslySelected, clickedHex))
                {
                    //SECOND CLICK
                    this.CmdMoveChar(previouslySelected, clickedHex);
                    this.UnselectHex();

                }
                else if (previouslySelected == clickedHex)
                {
                    this.UnselectHex();
                }
                break;
            case ControlMode.attack:
                if (previouslySelected == null)
                {
                    //FIRST CLICK
                    this.SelectHex(clickedHex);                    
                }
                else if (this.IsValidAttackForPlayer(GameController.Singleton.LocalPlayer.playerID, previouslySelected, clickedHex))
                {
                    //SECOND CLICK                    
                    this.CmdAttack(previouslySelected, clickedHex);
                    this.UnselectHex();
                } else if (previouslySelected == clickedHex)
                {
                    this.UnselectHex();
                }
                break;
        }
    }

    public void SelectHex(Hex h)
    {
        this.SelectedHex = h;
        h.drawer.Select(true);

        int heldCharacterID;
        PlayerCharacter heldCharacter;
        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                heldCharacterID = h.holdsCharacterWithClassID;
                heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
                this.DisplayMovementRange(h, heldCharacter.CanMoveDistance());
                break;
            case ControlMode.attack:
                heldCharacterID = h.holdsCharacterWithClassID;
                heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
                this.DisplayAttackRange(h, heldCharacter.currentStats.range);
                break;
        }
    }

    public void UnselectHex()
    {
        if (this.SelectedHex == null) { return; }
        this.SelectedHex.drawer.Select(false);
        this.SelectedHex = null;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                this.HideMovementRange();
                this.HidePath();
                break;
            case ControlMode.attack:
                this.HideAttackRange();
                break;
        }
    }

    public void HoverHex(Hex hoveredHex)
    {
        this.HoveredHex = hoveredHex;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                hoveredHex.drawer.MoveHover(true);

                //find path to hex if we have selected another hex
                if (this.SelectedHex != null)
                {
                    this.HidePath();
                    List<Hex> path = this.FindMovementPath(this.SelectedHex, hoveredHex);
                    if (path != null)
                    {
                        this.DisplayPath(path);
                    }
                }
                break;
            case ControlMode.attack:
                hoveredHex.drawer.AttackHover(true);
                break;
        }
    }

    public void UnhoverHex(Hex unhoveredHex)
    {
        //in case we somehow unhover a hex AFTER we starting hovering another        
        if (this.HoveredHex != unhoveredHex)
        {
            return;
        }

        this.HoveredHex = null;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                unhoveredHex.drawer.MoveHover(false);
                this.HidePath();
                break;
            case ControlMode.attack:
                unhoveredHex.drawer.AttackHover(false);
                break;
        }
    }

    private void DisplayMovementRange(Hex source, int moveDistance)
    {
        this.displayedMoveRange = RangeWithObstaclesAndCost(source, moveDistance);
        foreach (Hex h in this.displayedMoveRange)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                h.drawer.DisplayMoveRange(true);
            }
        }
    }

    private void HideMovementRange()
    {
        foreach (Hex h in this.displayedMoveRange)
        {
            h.drawer.DisplayMoveRange(false);
        }
    }

    private void DisplayPath(List<Hex> path)
    {
        //save path for hiding later
        this.displayedPath = path;
        int pathLength = 0;
        foreach (Hex h in path)
        {
            pathLength += h.MoveCost();

            //skip starting hex label
            if (pathLength != 0)
            {
                h.drawer.LabelString = pathLength.ToString();
                h.drawer.ShowLabel();
            }
        }
    }

    private void HidePath()
    {
        foreach (Hex h in this.displayedPath)
        {
            h.drawer.HideLabel();
        }
    }

    private void DisplayAttackRange(Hex source, int range)
    {
        Dictionary<Hex,LOSTargetType> attackRange = this.FindAttackRange(source, range);
        this.displayedAttackRange = attackRange;
        foreach (Hex h in attackRange.Keys)
        {
            //selected hex stays at selected color state
            if (h != source)
            {
                if (attackRange[h] == LOSTargetType.targetable)
                    h.drawer.DisplayAttackRange(true);
                else if (attackRange[h] == LOSTargetType.obstructing)
                    h.drawer.DisplayLOSObstruction(true);
                else if (attackRange[h] == LOSTargetType.unreachable)
                {
                    h.drawer.DisplayOutOfAttackRange(true);
                }
            }
        }
    }

    private void HideAttackRange()
    {
        foreach (Hex h in this.displayedAttackRange.Keys)
        {
            if (this.displayedAttackRange[h] == LOSTargetType.targetable)
                h.drawer.DisplayAttackRange(false);
            else if (this.displayedAttackRange[h] == LOSTargetType.obstructing)
                h.drawer.DisplayLOSObstruction(false);
            else if (this.displayedAttackRange[h] == LOSTargetType.unreachable)
            {
                h.drawer.DisplayOutOfAttackRange(false);
            }
        }
    }

    //Used by button
    public void SetControlModeAttack()
    {
        this.SetControlMode(ControlMode.attack);
    }

    //Used by button
    public void SetControlModeMove()
    {
        this.SetControlMode(ControlMode.move);
    }

    private void SetControlMode(ControlMode mode)
    {
        this.UnselectHex();
        this.HidePath();
        this.HideMovementRange();
        this.HideAttackRange();
        this.CurrentControlMode = mode;
        if (GameController.Singleton.IsItMyTurn())
        {
            HexCoordinates toSelectCoords = this.characterPositions[GameController.Singleton.ClassIdForPlayingCharacter()];
            Hex toSelect = Map.GetHex(this.hexGrid, toSelectCoords.X, toSelectCoords.Y);
            this.SelectHex(toSelect);
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
                Hex destHex = Map.GetHex(this.hexGrid, destCoords.X, destCoords.Y);
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
                        fringes[k - 1 + neighbour.MoveCost()].Add(neighbour);
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
                    int costToNeighbour = costsSoFar[h] + neighbour.MoveCost();
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
        foreach (HexCoordinates neighbourCoord in h.coordinates.NeighbhouringCoordinates())
        {
            Hex neighbour = Map.GetHex(this.hexGrid, neighbourCoord.X, neighbourCoord.Y);
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
        foreach (HexCoordinates neighbourCoord in h.coordinates.NeighbhouringCoordinates())
        {
            Hex neighbour = Map.GetHex(this.hexGrid, neighbourCoord.X, neighbourCoord.Y);
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
        foreach (Hex h in path)
        {
            pathCost += h.MoveCost();
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
                int newCost = costsSoFar[currentHex] + next.MoveCost();
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

    private Dictionary<Hex, LOSTargetType> FindAttackRange(Hex source, int range)
    {
        Dictionary<Hex,LOSTargetType> hexesInRange = new();
        List<Hex> allHexesInRange = this.RangeIgnoringObstacles(source, range);

        allHexesInRange.Sort(Comparer<Hex>.Create((Hex h1, Hex h2) => Map.HexDistance(source, h1).CompareTo(Map.HexDistance(source, h2))));
        foreach (Hex target in allHexesInRange)
        {

            bool unobstructed = true;
            RaycastHit2D[] hits;
            Vector2 sourcePos = source.transform.position;
            Vector2 targetPos = target.transform.position;
            Vector2 direction = targetPos - sourcePos;

            hits = Physics2D.RaycastAll(sourcePos, direction, direction.magnitude, hexMask);
            Array.Sort(hits, Comparer<RaycastHit2D>.Create((RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance)));
            int hitIndex = 0;
            foreach (RaycastHit2D hit in hits)
            {
                //first hit is always source hex
                if (hitIndex == 0)
                {
                    hitIndex++;
                    continue;
                }

                GameObject hitObject = hit.collider.gameObject;
                Hex hitHex = hitObject.GetComponent<Hex>();
                if (hitHex != null && hitHex.BreaksLOS(target.HoldsACharacter() ? target.holdsCharacterWithClassID : -1))
                {
                    if (!hexesInRange.ContainsKey(hitHex) || hexesInRange[hitHex] == LOSTargetType.targetable)
                        hexesInRange[hitHex] = LOSTargetType.obstructing;
                    unobstructed = false;
                    for (int i = hitIndex + 1; i < hits.Length; i++)
                    {
                        Hex nextHex = hits[i].collider.gameObject.GetComponent<Hex>();
                        if(!hexesInRange.ContainsKey(nextHex))
                            hexesInRange[nextHex] = LOSTargetType.unreachable;
                    }
                    break;
                }
                hitIndex++;
            }
            if (unobstructed)
                if(!hexesInRange.ContainsKey(target) || (hexesInRange.ContainsKey(target) && hexesInRange[target] != LOSTargetType.obstructing))
                    hexesInRange[target] = LOSTargetType.targetable;
        }
        return hexesInRange;
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

    public bool LOSReaches(Hex source, Hex target, int range)
    {
        if (Map.HexDistance(source, target) > range)
            return false;

        bool unobstructed = true;
        RaycastHit2D[] hits;
        Vector2 sourcePos = source.transform.position;
        Vector2 destPos = target.transform.position;
        Vector2 direction = destPos - sourcePos;

        hits = Physics2D.RaycastAll(sourcePos, direction, direction.magnitude, hexMask);
        foreach (RaycastHit2D hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            Hex hitHex = hitObject.GetComponent<Hex>();
            if (hitHex != null && hitHex != source && hitHex.BreaksLOS(target.HoldsACharacter() ? target.holdsCharacterWithClassID : -1))
            {
                //hitHex.DisplayLOSObstruction(true);
                unobstructed = false;
            }
        }
        return unobstructed;
    }
    #endregion

    #region Commands
    [Command(requiresAuthority = false)]
    public void CmdMoveChar(Hex source, Hex dest, NetworkConnectionToClient sender = null)
    {
        //Validation, should be done already on client side but we double check in case something changed since
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        if (!IsValidMoveForPlayer(playerID, source, dest))
        {
            Debug.Log("Client requested invalid move");
            return;
        }

        //Validate path
        PlayerCharacter toMove = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
        List<Hex> path = FindMovementPath(source, dest);
        if (path == null)
        {
            Debug.Log("Client requested move with no valid path to destination");
            return;
        }
        int moveCost = PathCost(path);
        if (moveCost > toMove.CanMoveDistance()) {
            Debug.Log("Client requested move outside his current range");
            return;
        }

        toMove.UseMoves(moveCost);
        if (toMove.CanMoveDistance() == 0)
        {
            GameController.Singleton.RpcGrayOutMoveButton(sender);
            if (!toMove.hasAttacked)
                this.RpcSetControlMode(sender, ControlMode.attack);
        }

        dest.holdsCharacterWithClassID = source.holdsCharacterWithClassID;
        this.characterPositions[source.holdsCharacterWithClassID] = dest.coordinates;

        source.ClearCharacter();

        this.RpcPlaceChar(toMove.gameObject, dest.transform.position);

    }

    [Command(requiresAuthority = false)]
    private void CmdAttack(Hex source, Hex target, NetworkConnectionToClient sender = null)
    {
        Debug.Log("Pikachu, attack!");

        //validate
        int playerID = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
        if (!IsValidAttackForPlayer(playerID, source, target))
        {
            Debug.Log("Client requested invalid attack");
            return;
        }

        PlayerCharacter attackingCharacter = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
        PlayerCharacter targetedCharacter = GameController.Singleton.playerCharacters[target.holdsCharacterWithClassID];

        //handles character states and attack logic
        //CombatManager.Attack(attackingCharacter, targetedCharacter);
        IAction attackAction = new DefaultAttackAction(attackingCharacter, targetedCharacter, source, target, attackingCharacter.currentStats, targetedCharacter.currentStats, playerID);
        if (!attackAction.Validate())
            Debug.Log("Could not validate DefaultAttackAction.");
        else
            attackAction.CmdUse();

        GameController.Singleton.RpcGrayOutAttackButton(sender);

        //prevent from move - attack - move       
        if (attackingCharacter.CanMoveDistance() > 0)
        {
            this.RpcSetControlMode(sender, ControlMode.move);
        } else
        {
            //prevent from move - attack - move
            GameController.Singleton.RpcGrayOutMoveButton(sender);
        }

        if (!attackingCharacter.HasRemainingActions())
        {
            GameController.Singleton.EndTurn();
        }
    }


    [Command(requiresAuthority = false)]
    public void CmdCreateCharOnBoard(int characterClassID, Hex destinationHex, NetworkConnectionToClient sender = null)
    {
        int ownerPlayerIndex = sender.identity.gameObject.GetComponent<PlayerController>().playerID;
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
        newChar.GetComponent<PlayerCharacter>().SetOwner(ownerPlayerIndex);
        NetworkServer.Spawn(newChar, connectionToClient);
        GameController.Singleton.playerCharactersNetIDs.Add(characterClassID, newChar.GetComponent<NetworkIdentity>().netId);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacterWithClassID = characterClassID;

        Map.Singleton.RpcPlaceChar(newChar, destinationWorldPos);
        this.MarkCharacterSlotAsPlaced(sender, characterClassID);

        this.characterPositions[characterClassID] = destinationHex.coordinates;

        GameController.Singleton.EndTurn();
    }
    #endregion

    #region RPCs
    //update client UI to prevent placing same character twice
    [TargetRpc]
    public void MarkCharacterSlotAsPlaced(NetworkConnectionToClient target, int classID)
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
    public void RpcClearUIStateForTurn()
    {
        this.SetControlMode(ControlMode.move);        
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

    [TargetRpc]
    private void RpcSetControlMode(NetworkConnectionToClient target, ControlMode mode)
    {
        this.SetControlMode(mode);
    }
    #endregion

    #region Utility

    private bool IsValidMoveForPlayer(int playerID, Hex source, Hex dest)
    {
        if (source == null ||
            dest == null ||
            source == dest ||
            !source.IsValidMoveSource() ||
            !dest.IsValidMoveDest() ||
!           GameController.Singleton.CanIControlThisCharacter(source.holdsCharacterWithClassID, playerID))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsValidAttackForPlayer(int playerID, Hex source, Hex target)
    {
        //Allows targeting your own characters if healer
        bool allowTargetingAnyCharacter = false;
        if(source != null && source.IsValidAttackSource())
        {
            PlayerCharacter attacker = GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID];
            if (attacker.currentStats.damageType == DamageType.healing)
            {
                allowTargetingAnyCharacter = true;
            }
        }

        if (source == null ||
            target == null ||
            (!allowTargetingAnyCharacter && source == target) ||
            !source.IsValidAttackSource() ||
            !target.IsValidAttackTarget() ||
            !GameController.Singleton.CanIControlThisCharacter(source.holdsCharacterWithClassID, playerID) ||
            (!allowTargetingAnyCharacter && GameController.Singleton.DoesHeOwnThisCharacter(playerID, target.holdsCharacterWithClassID)) ||
            !this.LOSReaches(source, target, GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID].currentStats.range))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static void SetHex(Dictionary<Vector2Int, Hex> grid, int x, int y, Hex h)
    {
        grid[new Vector2Int(x, y)] = h;
    }

    public static Hex GetHex(Dictionary<Vector2Int, Hex> grid, int x, int y)
    {
        if (grid.TryGetValue(new Vector2Int(x, y), out Hex toReturn))
        {
            return toReturn;
        }
        else
        {
            return null;
        }
    }

    public static void DeleteHex(Dictionary<Vector2Int, Hex> grid, int x, int y)
    {
        Hex toDelete = Map.GetHex(grid, x, y);
        toDelete.Delete();
        grid.Remove(new Vector2Int(x, y));
    }

    #endregion

    public void Update()
    {
        if (!hexesSpawnedOnClient && isServer && this.hexGrid != null)
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