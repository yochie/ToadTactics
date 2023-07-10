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
    private MapGenerator mapGenerator;

    [SerializeField]
    private MapInputHandler inputHandler;

    [SerializeField]
    public MapRangeDisplayer rangeDisplayer;
    #endregion

    #region Synced vars
    public readonly Dictionary<Vector2Int, Hex> hexGrid = new();
    private readonly SyncDictionary<Vector2Int, uint> hexGridNetIDs = new();

    //maps classID onto HexCoordinates
    public readonly SyncDictionary<int, HexCoordinates> characterPositions = new();

    #endregion

    #region Runtime state vars
    public static Map Singleton { get; private set; }

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

    #region Commands

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

        GameController.Singleton.NextTurn();
    }
    #endregion

    #region RPCs

    [TargetRpc]
    public void RpcUpdateSelectedHex(NetworkConnectionToClient target, Hex dest)
    {
        this.inputHandler.SelectHex(dest);
    }

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

    #region Utility
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