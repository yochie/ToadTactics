using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HexMouseEventTracker : MonoBehaviour, IPointerClickHandler
{
    #region Vars
    private Hex master;
    public Hex Master 
    { 
        private get { return this.master; }
        set { this.master = value; }
    }

    [SerializeField]
    private MapInputHandler inputHandler;

    #endregion

    #region Events

    private void OnMouseEnter()
    {
        this.inputHandler.HoverHex(master);
    }

    private void OnMouseExit()
    {
        this.inputHandler.UnhoverHex(master);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.inputHandler.ClickHex(master);
    }
    #endregion
}
