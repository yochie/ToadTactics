using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MapInputHandler : NetworkBehaviour
{

    [SerializeField]
    private MapRangeDisplayer rangeDisplayer;

    [SerializeField]
    private ActionPreviewer actionPreviewer;

    [SerializeField]
    private Ballista ballistaPrefab;

    [SerializeField]
    private Texture2D attackCursorTexture;
    
    [SerializeField] 
    private Texture2D moveCursorTexture;

    [SerializeField] 
    private Texture2D abilityCursorTexture;
    
    [SerializeField] 
    private Texture2D ballistaCursorTexture;

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

            case ControlMode.useBallista:
                ActionExecutor.Singleton.CmdUseBallista(this.SelectedHex, clickedHex);
                break;
        }
    }

    //Called on mode change to select playing character
    //Sets state and displays relevant action range
    public void SelectHex(Hex h)
    {
        this.UnselectHex();
        this.SelectedHex = h;

        if (!h.HoldsACharacter())
            throw new System.Exception("Selecting hex without character is currently unsupported. Programmer should fix.");

        int heldCharacterID = h.holdsCharacterWithClassID;
        PlayerCharacter heldCharacter = GameController.Singleton.PlayerCharactersByID[heldCharacterID];
        this.SetPlayingCharacter(heldCharacter);

        AnimationSystem.Singleton.Queue(this.DisplaySelectionState(h, heldCharacter));
    }

    private IEnumerator DisplaySelectionState(Hex h, PlayerCharacter heldCharacter)
    {
        h.drawer.Select(true);
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
            case ControlMode.useBallista:
                this.rangeDisplayer.DisplayBallistaRange(h, this.ballistaPrefab, heldCharacter);
                break;
            case ControlMode.useEquipment:
                Debug.Log("Trying to select hex while control mode is useTreasure(currently unsupported).");
                break;
        }
        yield break;
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
        this.rangeDisplayer.HideBallistaRange();
        this.rangeDisplayer.HideAbilityRange();
        this.rangeDisplayer.UnHighlightTargetedArea();

        this.actionPreviewer.RemoveActionPreview();
    }

    public void HoverHex(Hex hoveredHex)
    {

        this.HoveredHex = hoveredHex;
        if (!this.allowInput)
            return;
        List<Hex> targetedHexes;
        this.SetCursor(this.CurrentControlMode);
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
                    ActionExecutor.Singleton.CmdPreviewMoveTo(this.SelectedHex, hoveredHex);
                }
                break;
            case ControlMode.attack:
                Hex attackerHex = this.SelectedHex;
                CharacterStats attackerStats = this.playingCharacter.CurrentStats;
                AreaType attackAreaType = attackerStats.attackAreaType;
                int attackAreaScaler = attackerStats.attackAreaScaler;
                bool attackRequiresLOS = attackerStats.attacksRequireLOS;
                targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, attackAreaType, attackerHex, hoveredHex, attackAreaScaler);
                this.rangeDisplayer.HighlightTargetedArea(attackerHex, hoveredHex, attackAreaType, attackAreaScaler, attackRequiresLOS, targetedHexes);
                ActionExecutor.Singleton.CmdPreviewAttackAt(this.SelectedHex, hoveredHex);
                break;
            case ControlMode.useAbility:
                Hex userHex = this.SelectedHex;
                AreaType abilityAreaType = currentActivatedAbilityStats.areaType;
                int abilityAreaScaler = currentActivatedAbilityStats.areaScaler;
                bool abilityRequiresLOS = currentActivatedAbilityStats.requiresLOS;
                if (abilityAreaType != AreaType.ownTeam && abilityAreaType != AreaType.enemyTeam)
                    targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, abilityAreaType, userHex, hoveredHex, abilityAreaScaler);
                else
                    targetedHexes = AreaGenerator.GetHexesForTeam(Map.Singleton.hexGrid, Map.Singleton.characterPositions, GameController.Singleton.DraftedCharacterOwners, abilityAreaType, userHex);
                this.rangeDisplayer.HighlightTargetedArea(userHex, hoveredHex, abilityAreaType, abilityAreaScaler, abilityRequiresLOS, targetedHexes);
                ActionExecutor.Singleton.CmdPreviewAbilityAt(this.SelectedHex, hoveredHex, this.currentActivatedAbilityStats);
                break;
            case ControlMode.useBallista:
                Hex ballistaHex = this.SelectedHex;
                CharacterStats ballistaAttackerStats = this.playingCharacter.CurrentStats;
                AreaType ballistaAreaType = this.ballistaPrefab.attackAreaType;
                int ballistaAreaScaler = this.ballistaPrefab.attackAreaScaler;
                bool ballistaRequiresLOS = this.ballistaPrefab.attacksRequireLOS;
                targetedHexes = AreaGenerator.GetHexesInArea(Map.Singleton.hexGrid, ballistaAreaType, ballistaHex, hoveredHex, ballistaAreaScaler);
                this.rangeDisplayer.HighlightTargetedArea(ballistaHex, hoveredHex, ballistaAreaType, ballistaAreaScaler, ballistaRequiresLOS, targetedHexes);
                ActionExecutor.Singleton.CmdPreviewUseBallista(this.SelectedHex, hoveredHex);
                break;
        }
    }

    private void SetCursor(ControlMode controlMode)
    {
        Texture2D cursorTexture = this.GetCursorTextureForMode(controlMode);
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void SetCursorToDefault()
    {        
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private Texture2D GetCursorTextureForMode(ControlMode controlMode)
    {
        switch (controlMode)
        {
            case ControlMode.attack:
                return this.attackCursorTexture;
            case ControlMode.move:
                return this.moveCursorTexture;
            case ControlMode.useAbility:
                return this.abilityCursorTexture;
            case ControlMode.useBallista:
                return this.ballistaCursorTexture;
            default:
                return null;
        }    
    }

    public void UnhoverHex(Hex unhoveredHex)
    {
        //in case we somehow unhover a hex AFTER we starting hovering another, make sure we are unhovering correct hex      
        if (this.HoveredHex == unhoveredHex)
        {
            this.HoveredHex = null;
        }

        this.SetCursorToDefault();

        this.actionPreviewer.RemoveActionPreview();

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
            case ControlMode.useBallista:
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

    //toSelect allows forcing selecting specific hex in case local info might not be updated yet
    //e.g. right after a move has been done
    public void SetControlMode(ControlMode mode, Hex toSelect = null)
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
            || mode == ControlMode.useBallista
            || mode == ControlMode.useEquipment
            || mode == ControlMode.none))
        {
            if (toSelect == null)
                this.SelectHexForPlayingCharacter();
            else
                this.SelectHex(toSelect);
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
    public void TargetRpcSetControlMode(NetworkConnectionToClient target, ControlMode mode, Hex toSelect)
    {
        this.SetControlMode(mode, toSelect);
    }

    [ClientRpc]
    public void RpcSetControlModeOnAllClients(ControlMode mode)
    {
        this.SetControlMode(mode, null);
    }

    public void OnCharacterSheetDisplayed(int classID)
    {
        this.allowInput = false;
    }

    public void OnCharacterSheetClosed()
    {
        this.allowInput = true;
    }

    public void SetInputAllowed(bool value)
    {
        this.allowInput = value;
        if(this.HoveredHex != null)
            this.HoverHex(this.HoveredHex);
    }
}
