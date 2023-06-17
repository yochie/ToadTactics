using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class Hex : NetworkBehaviour
{
    //vars used by UI only, not synced
    private SpriteRenderer sprite;
    private Color hexColor;
    public Color HexColor {
        get { return this.hexColor; }
        set {
            this.hexColor = value;
            this.sprite.color = value;
        }
    }
    private TextMeshProUGUI coordLabelTextMesh;
    private TextMeshProUGUI labelTextMesh;
    private string labelString;
    public string LabelString {
        get { return labelString;  }
        set { 
            labelString = value;
            this.labelTextMesh.text = value;
        }
    }

    //state vars to sync
    [SyncVar]
    public HexCoordinates coordinates;
    [SyncVar]
    public bool isStartingZone;

    [SyncVar]
    public int holdsCharacterWithPrefabID;
    [SyncVar]
    public ObstacleType holdsObstacle;
    [SyncVar]
    public HazardType holdsHazard;
    [SyncVar]
    public bool holdsTreasure;
    [SyncVar]
    public Color baseColor;
    //0 is host
    //1 is client
    [SyncVar]
    public int startZoneForPlayerIndex;
    [SyncVar]
    public int moveCost;

    public bool IsSelectable
    {
        get
        {
            bool toReturn = false;
            switch (GameController.Singleton.currentGameMode)
            {
                case GameMode.characterPlacement:
                    toReturn = false;
                    break;
                case GameMode.gameplay:
                    if (this.holdsCharacterWithPrefabID != -1)
                    {
                        if (GameController.Singleton.IsItMyClientsTurn() &&
                            GameController.Singleton.IsItThisCharactersTurn(this.holdsCharacterWithPrefabID))
                        {
                            toReturn = true;
                        }
                        else
                        {
                            toReturn = false;
                        }
                    } else { toReturn = true; }
                    break;
            }
            return toReturn;
        }
    }


    [Server]
    public void Init(HexCoordinates hc, string name, Vector3 position, Vector3 scale, Quaternion rotation) {
        this.name = name;
        this.coordinates = hc;

        //default values
        this.isStartingZone = false;
        this.startZoneForPlayerIndex = -1;
        this.holdsCharacterWithPrefabID = -1;
        this.holdsObstacle = ObstacleType.none;
        this.holdsHazard = HazardType.none;
        this.holdsTreasure = false;
        this.baseColor = Map.Singleton.HEX_BASE_COLOR;
        this.moveCost = 1;

        //not currently needed as its set during instatiation, but kept in case
        //scale is needed
        this.transform.position = position;
        this.transform.localScale = scale;
        this.transform.rotation = rotation;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //Debug.LogFormat("Creating hex on client {0} {1} {2}",this.coordinates, this.coordinates.X, this.coordinates.Y);
        this.sprite = this.GetComponent<SpriteRenderer>();

        if(this.isStartingZone)
        {
            if ((this.isServer && this.startZoneForPlayerIndex == 0) || (!this.isServer && this.startZoneForPlayerIndex == 1))
            {
                this.baseColor = Map.Singleton.HEX_START_BASE_COLOR;

            } else
            {
                this.baseColor = Map.Singleton.HEX_OPPONENT_START_BASE_COLOR;
            }
        }

        this.HexColor = this.baseColor;

        //coordinates hidden by default using canvas group alpha
        //use that component in editor mode to display
        TextMeshProUGUI coordLabel = Instantiate<TextMeshProUGUI>(Map.Singleton.cellLabelPrefab);
        coordLabel.rectTransform.SetParent(Map.Singleton.coordCanvas.transform, false);
        coordLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        coordLabel.text = this.coordinates.X + " " + this.coordinates.Y;
        this.coordLabelTextMesh = coordLabel;


        //labels to display single number during navigation (range, etc)
        TextMeshProUGUI numLabel = Instantiate<TextMeshProUGUI>(Map.Singleton.cellLabelPrefab);
        numLabel.fontSize = 4;
        numLabel.rectTransform.SetParent(Map.Singleton.labelsCanvas.transform, false);
        numLabel.rectTransform.anchoredPosition =
            new Vector2(this.transform.position.x, this.transform.position.y);
        this.labelTextMesh = numLabel;
    }

    private void OnMouseEnter() {
        Map.Singleton.HoverHex(this);
    }

    private void OnMouseExit()
    {
        Map.Singleton.UnhoverHex(this);
    }

    private void OnMouseDown()
    {
        if(IsSelectable)
            Map.Singleton.ClickHex(this);
    }

    internal void ShowLabel()
    {
        this.labelTextMesh.alpha = 1;
    }

    internal void HideLabel()
    {
        this.labelTextMesh.alpha = 0;
    }

    public void Delete()
    {
        if(this.coordLabelTextMesh != null) { 
            Destroy(this.coordLabelTextMesh.gameObject); 
        }
        
        if (this.labelTextMesh != null)
        {
            Destroy(this.labelTextMesh.gameObject);
        }
        
        Destroy(this.gameObject);
    }
}
