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
    private bool dragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
        {
            this.dragging = true;
            this.dragStartPosition = this.spriteImage.transform.position;
            this.highlightImage.raycastTarget = false;
            this.spriteImage.raycastTarget = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.dragging)
            this.spriteImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.dragging)
        {
            this.dragging = false;
            this.highlightImage.raycastTarget = true;
            this.spriteImage.raycastTarget = true;
            this.spriteImage.transform.position = this.dragStartPosition;
            Hex destinationHex = this.mapInputHandler.HoveredHex;
            if (destinationHex == null) { return; }
            GameController.Singleton.LocalPlayer.CmdPlaceCharOnBoard(this.HoldsCharacterWithClassID, destinationHex);
        }
    }

    public bool IsDraggable()
    {       
        if (!(GameController.Singleton.CurrentPhaseID == GamePhaseID.characterPlacement))
            return false;

        if (!GameController.Singleton.ItsMyTurn())
            return false;

        if (this.HasBeenPlacedOnBoard)
            return false;

        if (!this.IsForSelf)
            return false;
       
        return true;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (this.dragging || this.IsDraggable())
            return;
        this.OnCharacterSheetDisplayedEvent.Raise(this.HoldsCharacterWithClassID);
    }

    public void SetMapInputHandler (MapInputHandler inputHandler)
    {
        this.mapInputHandler = inputHandler;
    }
}
