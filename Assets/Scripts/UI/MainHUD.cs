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
    private TextMeshProUGUI instructionLabel;

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
    private TextMeshProUGUI abilityButtonText;

    [SerializeField]
    private GameObject abilityCooldownIndicator;

    [SerializeField]
    private GameObject actionButtonsGrid;

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
    public void TargetRpcUpdateButtonsAfterAction(NetworkConnectionToClient target, List<ControlMode> interactableButtons, ControlMode toHighlight, bool onBallista)
    {

        this.ToggleInteractableButtons(interactableButtons, toHighlight);

        this.ballistaButton.SetActive(onBallista);

    }

    [TargetRpc]
    public void TargetRpcSetupButtonsForTurn(NetworkConnectionToClient target, List<ControlMode> interactableButtons, ControlMode toHighlight, string abilityName, int abilityCooldown, int usesRemaining, bool hasActiveAbility, bool onBallista)
    {
        this.ActivateGameplayButtons(true);
        this.ToggleInteractableButtons(interactableButtons, toHighlight);
        if (!hasActiveAbility)
        {
            this.abilityButton.SetActive(false);
        } else
        {
            this.abilityButtonText.text = String.Format("{0}", abilityName);
            this.UpdateAbilityCooldownIndicator(abilityCooldown, usesRemaining);
        }

        this.ballistaButton.SetActive(onBallista);
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

        buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
    }

    private void SetInteractableGameplayButton(ControlMode mode, bool state)
    {
        if(!this.gameplayButtons.ContainsKey(mode))
        {
            Debug.LogFormat("Could not find button for {0}", mode);
            return;
        }

        GameObject buttonObject = this.gameplayButtons[mode];
        if (state)
        {
            buttonObject.GetComponent<Button>().interactable = true;
            buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
        }
        else
        {
            buttonObject.GetComponent<Button>().interactable = false;
            buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();

        }
    }

    public void ToggleInteractableButtons(List<ControlMode> activeButtons, ControlMode toHighlight)
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
    }


    [Client]
    private void SetInteractableGameplayButtons(bool state)
    {
        foreach(GameObject buttonObject in this.gameplayButtons.Values)
        {
            Button button = buttonObject.GetComponent<Button>();
            button.interactable = state;
            if (!state)
                buttonObject.GetComponent<Image>().color = this.GetUnselectedColor();
        }
    }

    [Client]
    private void ActivateGameplayButtons(bool state)
    {
        foreach (GameObject buttonObject in this.gameplayButtons.Values)
        {
            buttonObject.SetActive(state);
        }
    }

    public void HighlightGameplayButton(ControlMode mode)
    {
        foreach (KeyValuePair<ControlMode, GameObject> controlModeToButton in this.gameplayButtons)
        {
            Image buttonImage = controlModeToButton.Value.GetComponent<Image>();
            if (controlModeToButton.Key == mode)

                buttonImage.color = this.GetSelectedColor(mode);
            else
                buttonImage.color = this.GetUnselectedColor();
        }
    }

    #region Events

    [Client]
    public void OnLocalPlayerTurnStart()
    {        
        this.instructionLabel.text = "Your turn";
        this.endTurnButton.SetActive(true);
    }

    [Client]
    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting...";
        this.endTurnButton.SetActive(false);
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay)
        {
            this.ActivateGameplayButtons(false);
        }
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
        GameController.Singleton.CmdNextTurn();
    }

    //For tests
    //TODO : remove along with button for final version
    public void OnTestButtonClicked()
    {
        this.CmdOnTestButtonClicked();
    }

    [Command(requiresAuthority = false)]
    private void CmdOnTestButtonClicked(NetworkConnectionToClient sender = null)
    {
        Debug.Log("pretending you lost round");
        GameController.Singleton.EndRound(sender.identity.GetComponent<PlayerController>().playerID);
    }

    private Color GetSelectedColor(ControlMode controlMode)
    {
        Color activeColor = HexDrawer.HEX_IN_MOVE_RANGE_COLOR;
        switch (controlMode)
        {
            case ControlMode.move:
                activeColor = HexDrawer.HEX_IN_MOVE_RANGE_COLOR;
                break;
            case ControlMode.attack:
                activeColor = HexDrawer.HEX_ATTACK_TARGETABLE_COLOR;
                break;
            case ControlMode.useAbility:
                activeColor = HexDrawer.HEX_ABILITY_TARGETABLE_COLOR;
                break;
        }

        return activeColor;
    }

    private Color GetUnselectedColor()
    {
        return Utility.DEFAULT_BUTTON_COLOR;
    }

}
