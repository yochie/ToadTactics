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
    public void TargetRpcGrayOutMoveButton(NetworkConnectionToClient target)
    {
        this.moveButton.GetComponent<Button>().interactable = false;
        this.moveButton.GetComponent<Image>().color = Color.white;
    }

    [TargetRpc]
    public void TargetRpcGrayOutAttackButton(NetworkConnectionToClient target)
    {
        this.attackButton.GetComponent<Button>().interactable = false;
        this.attackButton.GetComponent<Image>().color = Color.white;
    }

    [TargetRpc]
    public void TargetRpcGrayOutAbilityButton(NetworkConnectionToClient target)
    {
        this.abilityButton.GetComponent<Button>().interactable = false;
        this.abilityButton.GetComponent<Image>().color = Color.white;
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

        //this.moveButton.GetComponent<Button>().interactable = state;
        //this.attackButton.GetComponent<Button>().interactable = state;
        //this.abilityButton.GetComponent<Button>().interactable = state;
        //if (!state)
        //{
        //    this.moveButton.GetComponent<Image>().color = Color.white;
        //    this.attackButton.GetComponent<Image>().color = Color.white;
        //    this.abilityButton.GetComponent<Image>().color = Color.white;
        //}
    }

    [Client]
    private void SetActiveGameplayButtons(bool state)
    {
        foreach (GameObject buttonObject in this.gameplayButtons.Values)
        {
            buttonObject.SetActive(state);
        }
        //this.moveButton.SetActive(state);
        //this.attackButton.SetActive(state);
        //this.abilityButton.SetActive(state);
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
        //switch (mode)
        //{
        //    case ControlMode.move:
        //        this.moveButton.GetComponent<Image>().color = Color.green;
        //        this.attackButton.GetComponent<Image>().color = Color.white;
        //        this.abilityButton.GetComponent<Image>().color = Color.white;
        //        break;
        //    case ControlMode.attack:
        //        this.moveButton.GetComponent<Image>().color = Color.white;
        //        this.attackButton.GetComponent<Image>().color = Color.green;
        //        this.abilityButton.GetComponent<Image>().color = Color.white;
        //        break;
        //    case ControlMode.useAbility:
        //        this.moveButton.GetComponent<Image>().color = Color.white;
        //        this.attackButton.GetComponent<Image>().color = Color.white;
        //        this.abilityButton.GetComponent<Image>().color = Color.green;
        //        break;
        //}
    }

    #region Events

    [Client]
    public void OnLocalPlayerTurnStart()
    {
        //todo: display "Its your turn" msg
        this.instructionLabel.text = "Your turn";
        this.endTurnButton.SetActive(true);
        if (GameController.Singleton.currentPhaseID == GamePhaseID.gameplay)
        {
            this.SetActiveGameplayButtons(true);
        }
    }

    [Client]
    public void OnLocalPlayerTurnEnd()
    {
        //todo : display "Waiting for other player" msg
        this.instructionLabel.text = "Waiting...";
        this.endTurnButton.SetActive(false);
        if (GameController.Singleton.currentPhaseID == GamePhaseID.gameplay)
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

        if (GameController.Singleton.ItsMyTurn())
        {
            this.SetInteractableGameplayButtons(true);
        }
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

}
