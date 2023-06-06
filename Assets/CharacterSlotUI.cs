using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.position = this.startPosition;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        this.startPosition = this.transform.position;
    }
}
