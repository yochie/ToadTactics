using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameController gc;
    public Map map;
    private Vector3 startPosition;
    public int HoldsPlayerCharacterWithIndex { get; set; }

    public bool IsDraggable {get { return this.gc.IsItMyTurn(); }}

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
            Hex destinationHex = this.map.HoveredHex;
            //Debug.Log(this.map);
            //Debug.Log(destinationHex);
            //Debug.Log(destinationHex.IsStartingZone);
            //Debug.Log(HoldsPlayerCharacterWithIndex);
            //Debug.Log(destination);
            if (destinationHex == null) { return; }
            this.gc.LocalPlayer.CmdCreateCharOnBoard(this.HoldsPlayerCharacterWithIndex, destinationHex);
        }
    }
}
