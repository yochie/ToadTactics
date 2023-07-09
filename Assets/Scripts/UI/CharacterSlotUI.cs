using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;


//RTODO: SetHighlight color moved here
//RTODO: IsDraggable should be set in gamecontroller when turn is started during correct phase (maybe an event?)
//RTODO: Character creation should be handled by playerController since he stores the PlayerCharacter thereafter. Means he should maybe be registerd here as observer of drag.
public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private MapInputHandler mapInputHandler;

    private Vector3 dragStartPosition;
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

    public bool IsDraggable() { 
        bool toReturn = false;
        switch (GameController.Singleton.currentPhase)
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
        if (this.IsDraggable())
            this.dragStartPosition = this.transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
            this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
        {
            this.transform.position = this.dragStartPosition;
            Hex destinationHex = this.mapInputHandler.HoveredHex;
            if (destinationHex == null) { return; }
            Map.Singleton.CmdCreateCharOnBoard(this.HoldsCharacterWithClassID, destinationHex);
        }
    }
}
