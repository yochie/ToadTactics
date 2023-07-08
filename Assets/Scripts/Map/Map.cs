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
                    List<Hex> path = MapPathfinder.FindMovementPath(this.SelectedHex, hoveredHex, this.hexGrid);
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
        this.displayedMoveRange = MapPathfinder.RangeWithObstaclesAndMoveCost(source, moveDistance, this.hexGrid);
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
        Dictionary<Hex,LOSTargetType> attackRange = MapPathfinder.FindAttackRange(source, range, this.hexGrid);
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
        List<Hex> path = MapPathfinder.FindMovementPath(source, dest, this.hexGrid);
        if (path == null)
        {
            Debug.Log("Client requested move with no valid path to destination");
            return;
        }
        int moveCost = MapPathfinder.PathCost(path);
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
            !MapPathfinder.LOSReaches(source, target, GameController.Singleton.playerCharacters[source.holdsCharacterWithClassID].currentStats.range))
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