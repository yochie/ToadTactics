using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HexInputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    #region Content
    public Hex master;
    #endregion

    #region Events

    private void OnMouseEnter()
    {
        Map.Singleton.HoverHex(master);
    }

    private void OnMouseExit()
    {
        Map.Singleton.UnhoverHex(master);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (master.IsClickable())
            Map.Singleton.ClickHex(master);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (master.IsDraggable())
        {
            master.dragStartPosition = this.transform.position;
            master.draggingStarted = true;
            Map.Singleton.StartDragHex(master);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (master.draggingStarted)
        {
            PlayerCharacter heldCharacter = master.GetHeldCharacterObject();
            heldCharacter.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (master.draggingStarted)
        {
            master.draggingStarted = false;
            PlayerCharacter heldCharacter = master.GetHeldCharacterObject();
            heldCharacter.transform.position = master.dragStartPosition;
            Map.Singleton.EndDragHex(master);
        }
    }
    #endregion
}
