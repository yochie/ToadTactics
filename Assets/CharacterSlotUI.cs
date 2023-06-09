using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Map map;
    private Vector3 startPosition;
    public int HoldsPlayerCharacterWithIndex { get; set; }
    public PlayerController LocalPlayer { get; set; }
    void Start()
    {
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        this.startPosition = this.transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        this.transform.position = this.startPosition;
        Hex destinationHex = this.map.HoveredHex;
        //Debug.Log(this.map);
        //Debug.Log(destinationHex);
        //Debug.Log(destinationHex.IsStartingZone);
        if (destinationHex == null || !destinationHex.isStartingZone)
        {
            Debug.Log("Invalid character destination");
            return;
        }
        Vector3 destination = destinationHex.transform.position;
        //Debug.Log(HoldsPlayerCharacterWithIndex);
        //Debug.Log(destination);
        this.LocalPlayer.CmdCreateChar(HoldsPlayerCharacterWithIndex, destination);
    }
}
