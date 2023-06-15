using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    public int HoldsPlayerCharacterWithIndex { get; set; }

    public bool IsDraggable {get { return GameController.Singleton.IsItMyTurn(); }}

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
            GameController.Singleton.LocalPlayer.CmdCreateCharOnBoard(this.HoldsPlayerCharacterWithIndex, destinationHex);
        }
    }
}
