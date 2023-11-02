using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MapInputHandler : NetworkBehaviour
{

    [SerializeField]
    private MapRangeDisplayer rangeDisplayer;

    public static MapInputHandler Singleton { get; private set; }

    private PlayerCharacter playingCharacter;
    public Hex SelectedHex { get; set; }
    public Hex HoveredHex { get; set; }

    public ControlMode CurrentControlMode
    {
        get;
        private set;
    }

    private CharacterAbilityStats currentActivatedAbilityStats;
    private EquipmentSO currentActivedEquipmentStats;

    private bool allowInput = true;
    
    private void Awake()
    {
        if (MapInputHandler.Singleton == null)
            MapInputHandler.Singleton = this;
        GameController.Singleton.mapInputHandler = this;
        Debug.Log("MapInputHandler awakened");
        this.playingCharacter = null;
        this.SelectedHex = null;
        this.HoveredHex = null;      
        this.CurrentControlMode = ControlMode.none;
        this.allowInput = true;
    }

    public void SetPlayingCharacter(PlayerCharacter character)
    {
        this.playingCharacter = character;
    }

    public void ClickHex(Hex clickedHex)
    {
        if (!this.allowInput)
            return;

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
            case ControlMode.useAbility:
                //TODO :remove after implementation of all abilities
                if (currentActivatedAbilityStats.stringID == "fake")
                    return;
                ActionExecutor.Singleton.CmdUseAbility(this.SelectedHex, clickedHex, this.currentActivatedAbilityStats);
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

        if (!h.HoldsACharacter())
            throw new System.Exception("Selecting hex without character is currently unsupported. Programmer should fix.");

        int heldCharacterID = h.holdsCharacterWithClassID;
        PlayerCharacter heldCharacter = GameController.Singleton.PlayerCharactersByID[heldCharacterID];
        this.SetPlayingCharacter(heldCharacter);
        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                //Debug.Log("Trying to select hex while control mode is none.");
                break;
            case ControlMode.characterPlacement:
                Debug.Log("Trying to select hex while control mode is characterPlacement.");
                break;
            case ControlMode.move:
                this.rangeDisplayer.DisplayMovementRange(h, heldCharacter.RemainingMoves);
                break;
            case ControlMode.attack:
                this.rangeDisplayer.DisplayAttackRange(h, heldCharacter.CurrentStats.range, heldCharacter);
                break;
            case ControlMode.useAbility:
                this.rangeDisplayer.DisplayAbilityRange(h, currentActivatedAbilityStats, heldCharacter);
                //Debug.Log("Trying to select hex while control mode is useAbility (currently unsupported).");
                break;
            case ControlMode.useEquipment:
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

        this.rangeDisplayer.HidePath();
        this.rangeDisplayer.HideMovementRange();
        this.rangeDisplayer.HideAttackRange();
        this.rangeDisplayer.HideAbilityRange();
        this.rangeDisplayer.UnHighlightTargetedArea();
    }

    public void HoverHex(Hex hoveredHex)
    {
        if (!this.allowInput)
            return;

        this.HoveredHex = hoveredHex;

        switch (this.CurrentControlMode)
        {
            case ControlMode.none:
                hoveredHex.drawer.MoveHover(true);
                break;
            case ControlMode.characterPlacement:            
                hoveredHex.drawer.MoveHover(true);
                break;
            case ControlMode.move:
                hoveredHex.drawer.MoveHover(true);

                //find path to hex if we have selected another hex
                if (this.SelectedHex != null)
                {
                    this.rangeDisplayer.HidePath();
                    List<Hex> path = MapPathfinder.FindMovementPath(this.SelectedHex, hoveredHex, Map.Singleton.hexGrid);
                    if (path != null)
                    {
                        this.rangeDisplayer.DisplayPath(path);
                    }
                }
                break;
            case ControlMode.attack:
                Hex attackerHex = this.SelectedHex;
                CharacterStats attackerStats = this.playingCharacter.CurrentStats;
                AreaType attackAreaType = attackerStats.attackAreaType;
                int attackAreaScaler = attackerStats.attackAreaScaler;
                bool attackRequiresLOS = attackerStats.attacksRequireLOS;
                this.rangeDisplayer.HighlightTargetedArea(attackerHex, hoveredHex, attackAreaType, attackAreaScaler, attackRequiresLOS);
                break;
            case ControlMode.useAbility:
                Hex userHex = this.SelectedHex;
                AreaType abilityAreaType = currentActivatedAbilityStats.areaType;
                int abilityAreaScaler = currentActivatedAbilityStats.areaScaler;
                bool abilityRequiresLOS = currentActivatedAbilityStats.requiresLOS;
                this.rangeDisplayer.HighlightTargetedArea(userHex, hoveredHex, abilityAreaType, abilityAreaScaler, abilityRequiresLOS);
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
                unhoveredHex.drawer.MoveHover(false);
                break;
            case ControlMode.characterPlacement:
                unhoveredHex.drawer.MoveHover(false);
                break;
            case ControlMode.move:
                unhoveredHex.drawer.MoveHover(false);
                this.rangeDisplayer.HidePath();
                break;
            case ControlMode.attack:
                this.rangeDisplayer.UnHighlightTargetedArea();
                break;
            case ControlMode.useAbility:
                this.rangeDisplayer.UnHighlightTargetedArea();
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

    //Used by button
    public void SetControlModeAbility()
    {
        this.SetControlMode(ControlMode.useAbility);
    }

    //Used by button
    public void SetControlModeBallista()
    {
        this.SetControlMode(ControlMode.useBallista);
    }

    public void SetControlMode(ControlMode mode)
    {
        MainHUD.Singleton.HighlightGameplayButton(mode);
        this.UnselectHex();
        this.CurrentControlMode = mode;

        if (mode == ControlMode.characterPlacement)
            return;

        if (mode == ControlMode.useAbility)
        {
            int classID = GameController.Singleton.GetCharacterIDForTurn();
            PlayerCharacter currentlyPlayingCharacter = GameController.Singleton.PlayerCharactersByID[classID];

            //TODO: fetch correct ability here instead of juste getting first one
            if (currentlyPlayingCharacter.charClass.abilities == null ||
                currentlyPlayingCharacter.charClass.abilities.Count < 1 ||
                currentlyPlayingCharacter.charClass.abilities[0].isPassive)
            {
                throw new System.Exception("Trying to use ability while character has no defined activated abilities to fetch.");
            }                
             else
            {
                //TODO : fetch actual requested ability instead of just grabbing first
                CharacterAbilityStats abilityStats = currentlyPlayingCharacter.charClass.abilities[0];
                this.currentActivatedAbilityStats = abilityStats;
            }
        }

        if (GameController.Singleton.ItsMyTurn() &&
            (mode == ControlMode.move
            || mode == ControlMode.attack
            || mode == ControlMode.useAbility
            || mode == ControlMode.useEquipment
            || mode == ControlMode.none))
        {
            this.SelectHexForPlayingCharacter();
        }
    }

    private void SelectHexForPlayingCharacter()
    {
        HexCoordinates toSelectCoords = Map.Singleton.characterPositions[GameController.Singleton.GetCharacterIDForTurn()];
        Hex toSelect = Map.GetHex(Map.Singleton.hexGrid, toSelectCoords.X, toSelectCoords.Y);
        this.SelectHex(toSelect);
    }

    [TargetRpc]
    public void TargetRpcSelectHex(NetworkConnectionToClient target, Hex toSelect)
    {
        this.SelectHex(toSelect);
    }

    [TargetRpc]
    public void TargetRpcSetControlMode(NetworkConnectionToClient target, ControlMode mode)
    {
        this.SetControlMode(mode);
    }

    [ClientRpc]
    public void RpcSetControlModeOnAllClients(ControlMode mode)
    {
        this.SetControlMode(mode);
    }

    public void OnCharacterSheetDisplayed(int classID)
    {
        this.allowInput = false;
    }

    public void OnCharacterSheetClosed()
    {
        this.allowInput = true;
    }

}
