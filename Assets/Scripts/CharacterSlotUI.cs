using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    public int HoldsCharacterWithPrefabID { get; set; }

    public bool hasBeenPlacedOnBoard = false;

    public bool IsDraggable {
        get {
            bool toReturn = false;
            switch (GameController.Singleton.currentGameMode)
            {
                case GameMode.characterPlacement:
                    if (GameController.Singleton.IsItMyClientsTurn() &&
                        !this.hasBeenPlacedOnBoard)
                    {
                        toReturn = true;
                    } else
                    {
                        toReturn = false;
                    }
                    break;
                case GameMode.gameplay:
                    toReturn = false;
                    break;
                case GameMode.draft:
                    break;
                case GameMode.treasureDraft:
                    break;
                case GameMode.treasureEquip:
                    break;
            }
            return toReturn;
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
            Map.Singleton.CmdCreateCharOnBoard(this.HoldsCharacterWithPrefabID, destinationHex);
        }
    }
}
