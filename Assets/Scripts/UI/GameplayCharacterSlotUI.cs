using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;
using System;

public class GameplayCharacterSlotUI : ActiveCharacterSlotUI, IBeginDragHandler, IDragHandler, IEndDragHandler, IOwnedByPlayer
{
    [SerializeField]
    private Image highlightImage;

    private MapInputHandler mapInputHandler;

    private bool hasBeenPlacedOnBoard = false;
    public bool HasBeenPlacedOnBoard
    {
        get { return this.hasBeenPlacedOnBoard; }
        set { 
            this.hasBeenPlacedOnBoard = value;
            this.IsHighlighted = !value;
        }
    }
    
    private bool isHighlighted = false;
    public bool IsHighlighted
    {
        get { return this.isHighlighted; }
        set
        {
            this.isHighlighted = value;
            this.highlightImage.gameObject.SetActive(value);
        }
    }

    public bool IsForSelf { get; set; }

    private Vector3 dragStartPosition;
    private bool dragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
        {
            this.dragStartPosition = this.spriteImage.transform.position;
            this.highlightImage.raycastTarget = false;
            this.spriteImage.raycastTarget = false;
        }
        this.dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
            this.spriteImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.dragging = false;
        this.highlightImage.raycastTarget = true;
        this.spriteImage.raycastTarget = true;

        if (this.IsDraggable())
        {
            this.spriteImage.transform.position = this.dragStartPosition;
            Hex destinationHex = this.mapInputHandler.HoveredHex;
            if (destinationHex == null) { return; }
            GameController.Singleton.LocalPlayer.CmdPlaceCharOnBoard(this.HoldsCharacterWithClassID, destinationHex);
        }
    }

    public bool IsDraggable()
    {
        bool toReturn = false;
        switch (GameController.Singleton.CurrentPhaseID)
        {
            case GamePhaseID.characterPlacement:
                if (GameController.Singleton.ItsMyTurn() &&
                    !this.HasBeenPlacedOnBoard &&
                    this.IsForSelf)
                {
                    toReturn = true;
                }
                else
                {
                    toReturn = false;
                }
                break;
            case GamePhaseID.gameplay:
                break;
            case GamePhaseID.characterDraft:
                break;
            case GamePhaseID.equipmentDraft:
                break;
        }
        return toReturn;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (this.dragging)
            return;
        this.OnCharacterSheetDisplayedEvent.Raise(this.HoldsCharacterWithClassID);
    }

    public void SetMapInputHandler (MapInputHandler inputHandler)
    {
        this.mapInputHandler = inputHandler;
    }
}
