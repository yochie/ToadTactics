using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MapGenerator : MonoBehaviour
{
    #region Editor vars
    [SerializeField]
    private bool isFlatTop;
    //radius in hex count
    [SerializeField]
    private int xSize;
    [SerializeField]
    private int ySize;
    [SerializeField]
    private int obstacleSpawnPercent;

    [SerializeField]
    private MapOutline outline;
    [SerializeField]
    private TreasureGenerator treasureGenerator;
    [SerializeField]
    private List<StartZone> startingZones;

    [SerializeField]
    private GameObject hexPrefab;
    [SerializeField]
    private GameObject treePrefab;

    //corner to corner, or width (two times side length)
    //should correspond to unscaled sprite width
    [SerializeField]
    private float hexWidth = 1f;
    //flat to flat, or height, calculated on init by WIDTH_TO_HEIGHT_RATIO
    private float hexHeight;
    //geometric property of hexes
    private const float WIDTH_TO_HEIGHT_RATIO = 1.155f;

    [SerializeField]
    private float padding = 0.1f;

    #endregion

    #region Runtime vars
    private Dictionary<Vector2Int, Hex> generatedHexes;
    #endregion

    [Server]
    public Dictionary<Vector2Int, uint> GenerateMap()
    {
        this.hexHeight = this.hexWidth / WIDTH_TO_HEIGHT_RATIO;

        this.GenerateHexes();

        this.outline.DeleteHexesOutside(this.generatedHexes, this.xSize, this.ySize);

        //sets flag on hexes that are starting zones
        //also assigns player
        for (int i = 0; i < this.startingZones.Count; i++)
        {
            this.startingZones[i].SetStartingZone(this.generatedHexes, this.xSize, this.ySize);
        }

        this.treasureGenerator.GenerateTreasureAndBallista(this.generatedHexes);

        this.GenerateTrees();

        return SpawnHexesOnNetwork();
    }

    [Server]
    private void GenerateHexes()
    {
        this.generatedHexes = new();
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

                Map.SetHex(this.generatedHexes, x, y, h);
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
                Hex h = Map.GetHex(this.generatedHexes, x, y);
                if (h != null && !h.isStartingZone && !h.holdsTreasure && h.holdsHazard == HazardType.none)
                {
                    if (UnityEngine.Random.Range(0, 100) < this.obstacleSpawnPercent)
                    {
                        Map.Singleton.obstacleManager.SpawnObstacleOnMap(this.generatedHexes, h.coordinates.OffsetCoordinatesAsVector(), ObstacleType.tree);
                    }
                }
            }
        }
    }

    [Server]
    private Dictionary<Vector2Int, uint> SpawnHexesOnNetwork()
    {
        Dictionary<Vector2Int, uint> toReturn = new();

        //spawn all hexes on clients now that weve cleaned up extras and set all initial state
        for (int x = -this.xSize + 1; x < this.xSize; x++)
        {
            for (int y = -this.ySize + 1; y < this.ySize; y++)
            {
                Hex h = Map.GetHex(this.generatedHexes, x, y);
                if (h != null)
                {
                    NetworkServer.Spawn(h.gameObject);

                    //used to sync hexGrid using coroutine callbacks on client
                    //bypasses issues with syncing gameobjects that haven't been spawned yet
                    toReturn[new Vector2Int(x, y)] = h.gameObject.GetComponent<NetworkIdentity>().netId;
                }
            }
        }

        return toReturn;
    }
}
