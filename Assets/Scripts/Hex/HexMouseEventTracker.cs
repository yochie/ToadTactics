using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HexMouseEventTracker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        this.inputHandler.StartDragHex(master);

    }
    public void OnDrag(PointerEventData eventData)
    {
        this.inputHandler.DraggingHex(master, eventData);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        this.inputHandler.EndDragHex(master);
    }
    #endregion

    public bool IsClickable()
    {
        switch (GameController.Singleton.currentPhase)
        {
            case GamePhase.characterPlacement:
                return false;
            case GamePhase.gameplay:
                switch (this.inputHandler.CurrentControlMode)
                {
                    case ControlMode.move:
                        if (master.IsValidMoveSource() &&
                            GameController.Singleton.CanIControlThisCharacter(master.holdsCharacterWithClassID))
                            return true;
                        else if (this.inputHandler.SelectedHex != null && master.IsValidMoveDest())
                            return true;
                        else
                            return false;
                    case ControlMode.attack:
                        if (master.IsValidAttackSource() &&
                            GameController.Singleton.CanIControlThisCharacter(master.holdsCharacterWithClassID))
                            return true;
                        else if (this.inputHandler.SelectedHex != null && master.IsValidAttackTarget())
                            return true;
                        else
                            return false;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    public bool IsDraggable()
    {
        switch (this.inputHandler.CurrentControlMode)
        {
            case ControlMode.move:
                if (master.IsValidMoveSource() &&
                    GameController.Singleton.CanIControlThisCharacter(master.holdsCharacterWithClassID))
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }
}
