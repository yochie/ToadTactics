using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[CreateAssetMenu]
public class MapInputHandler : ScriptableObject
{
    //to be used eventually...
    private PlayerCharacter playingCharacter;
    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }

    public ControlMode CurrentControlMode
    {
        get;
        private set;
    }

    private CharacterAbility currentAbilityStats;
    private Treasure currentTreasureStats;

    //used to reset all state
    private void OnEnable()
    {
        this.playingCharacter = null;
        this.SelectedHex = null;
        this.HoveredHex = null;
        this.currentAbilityStats = new CharacterAbility();
        this.currentTreasureStats = new Treasure();
        this.CurrentControlMode = ControlMode.none;
    }

    public void setPlayingCharacter(PlayerCharacter character)
    {
        this.playingCharacter = character;
    }

    public void ClickHex(Hex clickedHex)
    {
        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                return;
            case ControlMode.characterPlacement:
                return;
            case ControlMode.move:
                ActionExecutor.Singleton.CmdMoveChar(this.SelectedHex, clickedHex);
                break;
            case ControlMode.attack:                
                ActionExecutor.Singleton.CmdAttack(this.SelectedHex, clickedHex);
                break;
        }
    }

    //Called on mode change to select playing character
    //Sets state and displays relevant action range
    public void SelectHex(Hex h)
    {
        this.UnselectHex();
        this.SelectedHex = h;
        h.drawer.Select(true);

        int heldCharacterID = h.holdsCharacterWithClassID;
        PlayerCharacter heldCharacter = GameController.Singleton.playerCharacters[heldCharacterID];
        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                Debug.Log("Trying to select hex while control mode is none.");
                break;
            case ControlMode.characterPlacement:
                Debug.Log("Trying to select hex while control mode is characterPlacement.");
                break;
            case ControlMode.move:
                Map.Singleton.rangeDisplayer.DisplayMovementRange(h, heldCharacter.CanMoveDistance());
                break;
            case ControlMode.attack:
                Map.Singleton.rangeDisplayer.DisplayAttackRange(h, heldCharacter.currentStats.range);
                break;
            case ControlMode.useAbility:
                Debug.Log("Trying to select hex while control mode is useAbility (currently unsupported).");
                break;
            case ControlMode.useTreasure:
                Debug.Log("Trying to select hex while control mode is useTreasure(currently unsupported).");
                break;
        }
    }

    public void UnselectHex()
    {
        if (this.SelectedHex != null)
        {
            this.SelectedHex.drawer.Select(false);
        }
        this.SelectedHex = null;

        Map.Singleton.rangeDisplayer.HidePath();
        Map.Singleton.rangeDisplayer.HideMovementRange();
        Map.Singleton.rangeDisplayer.HideAttackRange();
    }

    public void HoverHex(Hex hoveredHex)
    {
        this.HoveredHex = hoveredHex;

        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                return;
            case ControlMode.characterPlacement:            
                hoveredHex.drawer.MoveHover(true);
                break;
            case ControlMode.move:
                hoveredHex.drawer.MoveHover(true);

                //find path to hex if we have selected another hex
                if (this.SelectedHex != null)
                {
                    Map.Singleton.rangeDisplayer.HidePath();
                    List<Hex> path = MapPathfinder.FindMovementPath(this.SelectedHex, hoveredHex, Map.Singleton.hexGrid);
                    if (path != null)
                    {
                        Map.Singleton.rangeDisplayer.DisplayPath(path);
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
        if (this.HoveredHex == unhoveredHex)
        {
            this.HoveredHex = null;
        }

        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                return;
            case ControlMode.characterPlacement:
                unhoveredHex.drawer.MoveHover(false);
                break;
            case ControlMode.move:
                unhoveredHex.drawer.MoveHover(false);
                Map.Singleton.rangeDisplayer.HidePath();
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
        this.CurrentControlMode = mode;
        if (GameController.Singleton.IsItMyTurn() &&
            (mode == ControlMode.move
            || mode == ControlMode.attack
            || mode == ControlMode.useAbility
            || mode == ControlMode.useTreasure))
        {
            this.updateSelectedHex();
        }
    }

    private void updateSelectedHex()
    {
        HexCoordinates toSelectCoords = Map.Singleton.characterPositions[GameController.Singleton.ClassIdForPlayingCharacter()];
        Hex toSelect = Map.GetHex(Map.Singleton.hexGrid, toSelectCoords.X, toSelectCoords.Y);
        this.SelectHex(toSelect);
    }
}
