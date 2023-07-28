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
    private GameObject actionButtonsGrid;

    public static MainHUD Singleton { get; set; }

    private void Awake()
    {
        MainHUD.Singleton = this;
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

    [Client]
    private void SetInteractableGameplayButtons(bool state)
    {
        this.moveButton.GetComponent<Button>().interactable = state;
        this.attackButton.GetComponent<Button>().interactable = state;
        if (!state)
        {
            this.moveButton.GetComponent<Image>().color = Color.white;
            this.attackButton.GetComponent<Image>().color = Color.white;
        }
    }

    [Client]
    private void SetActiveGameplayButtons(bool state)
    {
        this.moveButton.SetActive(state);
        this.attackButton.SetActive(state);
    }

    public void HighlightGameplayButton(ControlMode mode)
    {
        switch (mode)
        {
            case ControlMode.move:
                this.moveButton.GetComponent<Image>().color = Color.green;
                this.attackButton.GetComponent<Image>().color = Color.white;
                break;
            case ControlMode.attack:
                this.moveButton.GetComponent<Image>().color = Color.white;
                this.attackButton.GetComponent<Image>().color = Color.green;
                break;
        }
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

    [Client]
    public void OnInitGameplayMode()
    {
        if (GameController.Singleton.ItsMyTurn())
        {
            this.SetActiveGameplayButtons(true);
        }
    }

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
