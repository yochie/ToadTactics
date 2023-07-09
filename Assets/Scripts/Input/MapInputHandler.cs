using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[CreateAssetMenu]
public class MapInputHandler : ScriptableObject
{
    private PlayerCharacter playingCharacter;
    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }

    private ControlMode currentControlMode = ControlMode.move;
    public ControlMode CurrentControlMode
    {
        get { return this.currentControlMode; }
    }

    private CharacterAbility currentAbilityStats;
    private Treasure currentTreasureStats;

    private Vector3 dragStartPosition;
    private bool draggingStarted;

    public void setPlayingCharacter(PlayerCharacter character)
    {
        this.playingCharacter = character;
    }

    //only used for movement
    public void StartDragHex(Hex draggedHex)
    {
        if (draggedHex.inputHandler.IsDraggable())
        {
            this.dragStartPosition = draggedHex.transform.position;
            this.draggingStarted = true;
        }
        else
        {
            return;
        }

        if (draggedHex == null || !draggedHex.IsValidMoveSource())
        {
            this.UnselectHex();
        }
        else
        {
            this.UnselectHex();
            this.SelectHex(draggedHex);
        }
    }

    //only used for movement
    public void EndDragHex(Hex startHex)
    {
        if (!this.draggingStarted)
            return;

        PlayerCharacter heldCharacter = startHex.GetHeldCharacterObject();
        heldCharacter.transform.position = this.dragStartPosition;
        this.draggingStarted = false;

        Hex endHex = this.HoveredHex;

        if (endHex == null || !Map.Singleton.IsValidMoveForPlayer(GameController.Singleton.LocalPlayer.playerID, startHex, endHex))
        {
            this.UnselectHex();
            return;
        }

        Map.Singleton.CmdMoveChar(startHex, endHex);
        this.UnselectHex();
    }

    public void DraggingHex(Hex draggedHex, PointerEventData eventData)
    {
        if (this.draggingStarted)
        {
            PlayerCharacter heldCharacter = draggedHex.GetHeldCharacterObject();
            heldCharacter.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));
        }
    }

    public void ClickHex(Hex clickedHex)
    {
        if (!clickedHex.inputHandler.IsClickable())
            return;
        
        Hex previouslySelected = this.SelectedHex;
        switch (this.CurrentControlMode)
        {
            case ControlMode.move:

                if (previouslySelected == null)
                {
                    //FIRST CLICK
                    this.SelectHex(clickedHex);
                }
                else if (Map.Singleton.IsValidMoveForPlayer(GameController.Singleton.LocalPlayer.playerID, previouslySelected, clickedHex))
                {
                    //SECOND CLICK
                    Map.Singleton.CmdMoveChar(previouslySelected, clickedHex);
                    this.UnselectHex();

                }
                else if (previouslySelected == clickedHex)
                {
                    this.UnselectHex();
                }
                break;
            case ControlMode.attack:
                if (previouslySelected == null)
                {
                    //FIRST CLICK
                    this.SelectHex(clickedHex);
                }
                else if (Map.Singleton.IsValidAttackForPlayer(GameController.Singleton.LocalPlayer.playerID, previouslySelected, clickedHex))
                {
                    //SECOND CLICK                    
                    Map.Singleton.CmdAttack(previouslySelected, clickedHex);
                    this.UnselectHex();
                }
                else if (previouslySelected == clickedHex)
                {
                    this.UnselectHex();
                }
                break;
        }
    }

    public void SelectHex(Hex h)
    {
        this.SelectedHex = h;
        h.drawer.Select(true);

        int heldCharacterID;
        PlayerCharacter heldCharacter;
        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                heldCharacterID = h.holdsCharacterWithClassID;
                heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
                Map.Singleton.DisplayMovementRange(h, heldCharacter.CanMoveDistance());
                break;
            case ControlMode.attack:
                heldCharacterID = h.holdsCharacterWithClassID;
                heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
                Map.Singleton.DisplayAttackRange(h, heldCharacter.currentStats.range);
                break;
        }
    }

    public void UnselectHex()
    {
        if (this.SelectedHex == null) { return; }
        this.SelectedHex.drawer.Select(false);
        this.SelectedHex = null;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                Map.Singleton.HideMovementRange();
                Map.Singleton.HidePath();
                break;
            case ControlMode.attack:
                Map.Singleton.HideAttackRange();
                break;
        }
    }

    public void HoverHex(Hex hoveredHex)
    {
        this.HoveredHex = hoveredHex;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                hoveredHex.drawer.MoveHover(true);

                //find path to hex if we have selected another hex
                if (this.SelectedHex != null)
                {
                    Map.Singleton.HidePath();
                    List<Hex> path = MapPathfinder.FindMovementPath(this.SelectedHex, hoveredHex, Map.Singleton.hexGrid);
                    if (path != null)
                    {
                        Map.Singleton.DisplayPath(path);
                    }
                }
                break;
            case ControlMode.attack:
                hoveredHex.drawer.AttackHover(true);
                break;
        }
    }

    public void UnhoverHex(Hex unhoveredHex)
    {
        //in case we somehow unhover a hex AFTER we starting hovering another        
        if (this.HoveredHex != unhoveredHex)
        {
            return;
        }

        this.HoveredHex = null;

        switch (this.CurrentControlMode)
        {
            case ControlMode.move:
                unhoveredHex.drawer.MoveHover(false);
                Map.Singleton.HidePath();
                break;
            case ControlMode.attack:
                unhoveredHex.drawer.AttackHover(false);
                break;
        }
    }

    //Used by button
    public void SetControlModeAttack()
    {
        this.SetControlMode(ControlMode.attack);
    }

    //Used by button
    public void SetControlModeMove()
    {
        this.SetControlMode(ControlMode.move);
    }

    public void SetControlMode(ControlMode mode)
    {
        GameController.Singleton.HighlightGameplayButton(mode);
        this.UnselectHex();
        Map.Singleton.HidePath();
        Map.Singleton.HideMovementRange();
        Map.Singleton.HideAttackRange();
        this.currentControlMode = mode;
        if (GameController.Singleton.IsItMyTurn())
        {
            HexCoordinates toSelectCoords = Map.Singleton.characterPositions[GameController.Singleton.ClassIdForPlayingCharacter()];
            Hex toSelect = Map.GetHex(Map.Singleton.hexGrid, toSelectCoords.X, toSelectCoords.Y);
            this.SelectHex(toSelect);
        }
    }
}
