using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    public int HoldsCharacterWithPrefabID { get; set; }

    public bool IsDraggable {
        get {
            bool toReturn = false;
            switch (GameController.Singleton.currentGameMode)
            {
                case GameMode.characterPlacement:
                    toReturn = GameController.Singleton.IsItMyClientsTurn();
                    break;
                case GameMode.gameplay:
                    toReturn = true;
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
            GameController.Singleton.LocalPlayer.CmdCreateCharOnBoard(this.HoldsCharacterWithPrefabID, destinationHex);
        }
    }
}
