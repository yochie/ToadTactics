using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;
using System;

public class GameplayCharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private MapInputHandler mapInputHandler;

    [SerializeField]
    private GameObject crown;

    [SerializeField]
    private IntGameEventSO onCharacterSheetDisplayed;

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
    private bool dragging;

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
        this.dragging = true;
    }

    internal void DisplayCrown()
    {
        this.crown.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.IsDraggable())
            this.transform.position = eventData.position;
    }

    internal void SetInputHandler(MapInputHandler inputHandler)
    {
        this.mapInputHandler = inputHandler;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.dragging = false;

        if (this.IsDraggable())
        {
            this.transform.position = this.dragStartPosition;
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
                    !this.HasBeenPlacedOnBoard)
                {
                    toReturn = true;
                }
                else
                {
                    toReturn = false;
                }
                break;
            case GamePhaseID.gameplay:
                toReturn = false;
                break;
            case GamePhaseID.characterDraft:
                break;
            case GamePhaseID.equipmentDraft:
                break;
        }
        return toReturn;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this.dragging)
            return;
        this.onCharacterSheetDisplayed.Raise(this.HoldsCharacterWithClassID);
    }
}
