using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HexMouseEventTracker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    //private void OnMouseEnter()
    //{
    //    if (!EventSystem.current.IsPointerOverGameObject())
    //        return;
    //    this.inputHandler.HoverHex(master);
    //}

    //private void OnMouseExit()
    //{
    //    if (!EventSystem.current.IsPointerOverGameObject())
    //        return;
    //    this.inputHandler.UnhoverHex(master);
    //}

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.inputHandler.ClickHex(master);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        this.inputHandler.HoverHex(master);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        this.inputHandler.UnhoverHex(master);
    }
    #endregion

    private void Start()
    {
        this.inputHandler = Map.Singleton.GetComponent<MapInputHandler>();
    }
}
