using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class MainHUD : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI turnInstructionLabel;

    [SerializeField]
    private TextMeshProUGUI placementInstructionLabel;

    [SerializeField]
    private GameObject endTurnButton;

    [SerializeField]
    private GameObject moveButton;

    [SerializeField]
    private GameObject attackButton;

    [SerializeField]
    private GameObject abilityButton;

    [SerializeField]
    private GameObject ballistaButton;

    [SerializeField]
    private Button ballistaReloadButton;

    [SerializeField]
    private TextMeshProUGUI abilityButtonText;

    [SerializeField]
    private GameObject abilityCooldownIndicator;

    [SerializeField]
    private GameObject actionButtonsGrid;

    [SerializeField]
    private TreasureRevealPanel treasureRevealPanel;
    
    [SerializeField]
    private ColorPaletteSO colorPalette;

    private Dictionary<ControlMode, GameObject> gameplayButtons;
    
    public static MainHUD Singleton { get; set; }

    private void Awake()
    {
        MainHUD.Singleton = this;
        this.gameplayButtons = new();
        this.gameplayButtons.Add(ControlMode.move, this.moveButton);
        this.gameplayButtons.Add(ControlMode.attack, this.attackButton);
        this.gameplayButtons.Add(ControlMode.useAbility, this.abilityButton);
        this.gameplayButtons.Add(ControlMode.useBallista, this.ballistaButton);
    }

    [TargetRpc]
    public void TargetRpcUpdateButtonsAfterAction(NetworkConnectionToClient target, List<ControlMode> interactableButtons, ControlMode toHighlight, bool onBallista, bool ballistaNeedsReload, bool ballistaReloadAvailable)
    {
        this.ballistaButton.SetActive(onBallista && !ballistaNeedsReload);
        this.ballistaReloadButton.gameObject.SetActive(onBallista && ballistaNeedsReload);
        this.ToggleInteractableButtons(interactableButtons, toHighlight, ballistaReloadAvailable);

    }

    [TargetRpc]
    public void TargetRpcSetupButtonsForTurn(NetworkConnectionToClient target, List<ControlMode> interactableButtons, ControlMode toHighlight, string abilityName, int abilityCooldown, int usesRemaining, bool hasActiveAbility, bool onBallista, bool ballistaNeedsReload, bool ballistaReloadAvailable)
    {
        this.ActivateGameplayButtons(true);

        this.ballistaButton.SetActive(onBallista && !ballistaNeedsReload);
        this.ballistaReloadButton.gameObject.SetActive(onBallista && ballistaNeedsReload);

        this.ToggleInteractableButtons(interactableButtons, toHighlight, ballistaReloadAvailable);
        if (!hasActiveAbility)
        {
            this.abilityButton.SetActive(false);
        } else
        {
            this.abilityButtonText.text = String.Format("{0}", abilityName);
            this.UpdateAbilityCooldownIndicator(abilityCooldown, usesRemaining);
        }


    }

    [TargetRpc]
    public void TargetRpcUpdateAbilityCooldownIndicator(NetworkConnectionToClient target, int abilityCooldown, int usesRemaining)
    {
        this.UpdateAbilityCooldownIndicator(abilityCooldown, usesRemaining);
    }

    private void UpdateAbilityCooldownIndicator(int abilityCooldown, int usesRemaining)
    {
        if (abilityCooldown <= 0 && usesRemaining < 0)
        {
            this.abilityCooldownIndicator.SetActive(false);
            return;
        } else
        {
            this.abilityCooldownIndicator.SetActive(true);
        }

        string abilityCooldownString = abilityCooldown > 0 ? abilityCooldown.ToString() : "";
        this.abilityCooldownIndicator.GetComponent<CooldownIndicator>().SetCooldown(abilityCooldownString);
        
        string abilityUsesString = usesRemaining >= 0 ? string.Format("{0} left", usesRemaining) : "";
        this.abilityCooldownIndicator.GetComponent<CooldownIndicator>().SetUsesCount(abilityUsesString);
    }

    [TargetRpc]
    public void TargetRpcGrayOutMoveButton(NetworkConnectionToClient target)
    {
        GrayOutGameplayButton(ControlMode.move);

    }

    [TargetRpc]
    public void TargetRpcGrayOutAttackButton(NetworkConnectionToClient target)
    {
        this.GrayOutGameplayButton(ControlMode.attack);

    }

    [TargetRpc]
    public void TargetRpcGrayOutAbilityButton(NetworkConnectionToClient target)
    {
        this.GrayOutGameplayButton(ControlMode.useAbility);
    }

    private void GrayOutGameplayButton(ControlMode controlMode)
    {
        GameObject buttonObject = this.gameplayButtons[controlMode];
        buttonObject.GetComponent<Button>().interactable = false;

        //buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
    }

    private void SetInteractableGameplayButton(ControlMode mode, bool interactable)
    {
        if(!this.gameplayButtons.ContainsKey(mode))
        {
            Debug.LogFormat("Could not find button for {0}", mode);
            return;
        }

        GameObject buttonObject = this.gameplayButtons[mode];
        if (interactable)
        {
            buttonObject.GetComponent<Button>().interactable = true;
            //buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
        }
        else
        {
            buttonObject.GetComponent<Button>().interactable = false;
            //buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();

        }
    }

    public void ToggleInteractableButtons(List<ControlMode> activeButtons, ControlMode toHighlight, bool ballistaReloadAvailable)
    {
        foreach (KeyValuePair<ControlMode, GameObject> buttonByMode in this.gameplayButtons)
        {
            if (activeButtons.Contains(buttonByMode.Key))
            {
                SetInteractableGameplayButton(buttonByMode.Key, true);
            }
            else
            {
                SetInteractableGameplayButton(buttonByMode.Key, false);
            }

        }

        if (toHighlight != ControlMode.none)
            this.HighlightGameplayButton(toHighlight);
        this.ballistaReloadButton.interactable = ballistaReloadAvailable;
    }


    [Client]
    private void SetInteractableGameplayButtons(bool interactable)
    {
        foreach(GameObject buttonObject in this.gameplayButtons.Values)
        {
            Button button = buttonObject.GetComponent<Button>();
            button.interactable = interactable;
            //if (!interactable)
            //    buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
        }
    }

    [Client]
    private void ActivateGameplayButtons(bool state)
    {
        foreach (GameObject buttonObject in this.gameplayButtons.Values)
        {
            buttonObject.SetActive(state);
        }

        //since this is not mapped to a control state, it is handled seperately
        this.ballistaReloadButton.gameObject.SetActive(state);
    }

    public void HighlightGameplayButton(ControlMode mode)
    {
        foreach (var (controlMode, button) in this.gameplayButtons)
        {
            ColorBlock buttonColors = button.GetComponent<Button>().colors;
            if (controlMode == mode)

                buttonColors.normalColor = this.GetSelectedColor(mode);
            else
                buttonColors.normalColor = this.GetUnselectedColor();

            button.GetComponent<Button>().colors = buttonColors;
        }
    }

    #region Events

    [Client]
    public void OnLocalPlayerTurnStart()
    {        
        this.turnInstructionLabel.text = "Turn : You";
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.characterPlacement)
            this.SetPlacementInstuctionState(yourTurn: true);
        else
            this.endTurnButton.SetActive(true);
    }

    [Client]
    public void OnLocalPlayerTurnEnd()
    {
        this.turnInstructionLabel.text = "Turn : Opponent";
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.characterPlacement)
            this.SetPlacementInstuctionState(yourTurn: false);
        else
            this.endTurnButton.SetActive(false);
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay)
        {
            this.ActivateGameplayButtons(false);
        }
    }

    [TargetRpc]
    public void TargetRpcEnablePlacementInstruction(NetworkConnectionToClient target, bool yourTurn)
    {
        this.placementInstructionLabel.gameObject.SetActive(true);
        this.SetPlacementInstuctionState(yourTurn);
    }

    [ClientRpc]
    public void RpcDisablePlacementInstruction()
    {
        this.placementInstructionLabel.gameObject.SetActive(false);
    }

    private void SetPlacementInstuctionState(bool yourTurn)
    {
        if(yourTurn)
            this.placementInstructionLabel.text = "Drag character onto map";
        else
            this.placementInstructionLabel.text = "Opponent is placing character";
    }

    //TODO : Remove, its called by RPC/syncvar hook and checks recently set syncvar, big nono
    //TODO : could perhaps be replaced by new event : LocalPlayerCharacterTurnStart (other similar event should be renamed LocalPlayerTakeControl)
    public void OnTurnOrderIndexChanged(int newTurnIndex)
    {
        if (newTurnIndex == -1)
            return;

        this.SetInteractableGameplayButtons(true);

        PlayerCharacter newTurnCharacter = GameController.Singleton.PlayerCharactersByID[GameController.Singleton.GetCharacterIDForTurn(newTurnIndex)];

        //TODO : remove, i dont think its required
        if (!newTurnCharacter.CanMove)
            this.GrayOutGameplayButton(ControlMode.move);
    }
    #endregion

    public void OnEndTurnButtonClicked()
    {
        GameController.Singleton.CmdEndMyTurn();
    }

    private Color GetSelectedColor(ControlMode controlMode)
    {
        Color activeColor = this.colorPalette.HEX_IN_MOVE_RANGE_COLOR;
        switch (controlMode)
        {
            case ControlMode.move:
                activeColor = this.colorPalette.HEX_IN_MOVE_RANGE_COLOR;
                break;
            case ControlMode.attack:
                activeColor = this.colorPalette.HEX_ATTACK_TARGETABLE_COLOR;
                break;
            case ControlMode.useAbility:
                activeColor = this.colorPalette.HEX_ABILITY_TARGETABLE_COLOR;
                break;
        }

        return activeColor;
    }

    private Color GetUnselectedColor()
    {
        return Utility.DEFAULT_BUTTON_COLOR;
    }

    internal void DisplayTreasureRevealPanel(string equipmentID, bool display)
    {
        if (!display)
            this.treasureRevealPanel.Hide();
        else
        {
            this.treasureRevealPanel.FillWithEquipmentData(equipmentID);
            this.treasureRevealPanel.Show();
        }
    }

    public void OnRoundEnd()
    {
        this.ActivateGameplayButtons(false);
        this.endTurnButton.SetActive(false);
    }
}
