using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    public int HoldsCharacterWithClassID { get; set; }

    private bool hasBeenPlacedOnBoard = false;
    public bool HasBeenPlacedOnBoard
    {
        get { return this.hasBeenPlacedOnBoard; }
        set { 
            hasBeenPlacedOnBoard = value;
            this.IsHighlighted = !value;
        }
    }

    private Image highlightImage;
    private bool isHighlighted = false;
    public bool IsHighlighted
    {
        get { return isHighlighted; }
        set
        {
            isHighlighted = value;
            Color oldColor = this.highlightImage.color;
            this.highlightImage.color = Utility.SetHighlight(oldColor, value); ;
        }
    }

    public bool IsDraggable {
        get {
            bool toReturn = false;
            switch (GameController.Singleton.currentGamePhase)
            {
                case GamePhase.characterPlacement:
                    if (GameController.Singleton.IsItMyTurn() &&
                        !this.HasBeenPlacedOnBoard)
                    {
                        toReturn = true;
                    } else
                    {
                        toReturn = false;
                    }
                    break;
                case GamePhase.gameplay:
                    toReturn = false;
                    break;
                case GamePhase.draft:
                    break;
                case GamePhase.treasureDraft:
                    break;
                case GamePhase.treasureEquip:
                    break;
            }
            return toReturn;
        }
    }

    public void Awake()
    {
        foreach (Image child in this.GetComponentsInChildren<Image>())
        {
            if (child.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
            {
                this.highlightImage = child;
            }
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (this.IsDraggable)
            this.startPosition = this.transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (this.IsDraggable)
            this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.IsDraggable)
        {
            this.transform.position = this.startPosition;
            Hex destinationHex = Map.Singleton.HoveredHex;
            if (destinationHex == null) { return; }
            Map.Singleton.CmdCreateCharOnBoard(this.HoldsCharacterWithClassID, destinationHex);
        }
    }
}
