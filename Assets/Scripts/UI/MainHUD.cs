using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

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
    }

    [TargetRpc]
    public void TargetRpcToggleActiveButtons(NetworkConnectionToClient target, List<ControlMode> activeButtons, ControlMode toHighlight)
    {
        foreach(KeyValuePair<ControlMode, GameObject> buttonByMode in this.gameplayButtons)
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

    private void GrayOutGameplayButton(ControlMode mode)
    {
        GameObject buttonObject = this.gameplayButtons[mode];
        buttonObject.GetComponent<Button>().interactable = false;
        buttonObject.GetComponent<Image>().color = Color.white;
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
            buttonObject.GetComponent<Image>().color = Color.white;
        }
        else
        {
            buttonObject.GetComponent<Button>().interactable = false;
            buttonObject.GetComponent<Image>().color = Color.white;

        }
    }

    [Client]
    private void SetInteractableGameplayButtons(bool state)
    {
        foreach(GameObject buttonObject in this.gameplayButtons.Values)
        {
            Button button = buttonObject.GetComponent<Button>();
            button.interactable = state;
            if (!state)
                buttonObject.GetComponent<Image>().color = Color.white;
        }
    }

    [Client]
    private void SetActiveGameplayButtons(bool state)
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

                buttonImage.color = Color.green;
            else
                buttonImage.color = Color.white;
        }
    }

    #region Events

    [Client]
    public void OnLocalPlayerTurnStart()
    {        
        this.instructionLabel.text = "Your turn";
        this.endTurnButton.SetActive(true);
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay)
        {
            this.SetActiveGameplayButtons(true);
        }
    }

    [Client]
    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting...";
        this.endTurnButton.SetActive(false);
        if (GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay)
        {
            this.SetActiveGameplayButtons(false);
        }
    }

    //TODO : Remove, its called by RPC/syncvar hook and checks recently set syncvar, big nono,  i dont event think its necessary but needs to be tested
    [Client]
    public void OnInitGameplayMode()
    {
        if (GameController.Singleton.ItsMyTurn())
        {
            this.SetActiveGameplayButtons(true);
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

        if (!newTurnCharacter.CanMove)
            this.GrayOutGameplayButton(ControlMode.move);

        //if (GameController.Singleton.ItsMyTurn())
        //{
        //    this.SetInteractableGameplayButtons(true);
        //}
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

    //private void Update()
    //{
    //    if(GameController.Singleton.ItsMyTurn())
    //        this.

                
    //    if (!canTakeTurn)
    //        return;
    //    if (!canMove)
    //        this.GrayOutGameplayButton(ControlMode.move);            
    //}

}
