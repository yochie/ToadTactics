using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MainHUD : NetworkBehaviour
{
    [SerializeField]
    public TextMeshProUGUI phaseLabel;

    [SerializeField]
    private GameObject endTurnButton;

    [SerializeField]
    private GameObject moveButton;

    [SerializeField]
    private GameObject attackButton;

    [SerializeField]
    private GameObject actionButtonsGrid;

    [SerializeField]
    private IntGameEventSOListener onTurnOrderIndexChangedListener;

    public static MainHUD Singleton { get; set; }

    private void Awake()
    {
        MainHUD.Singleton = this;

        onTurnOrderIndexChangedListener.Response.AddListener(OnTurnOrderIndexChanged);
    }

    [TargetRpc]
    public void RpcGrayOutMoveButton(NetworkConnectionToClient target)
    {
        this.moveButton.GetComponent<Button>().interactable = false;
        this.moveButton.GetComponent<Image>().color = Color.white;

    }

    [TargetRpc]
    public void RpcGrayOutAttackButton(NetworkConnectionToClient target)
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
        this.endTurnButton.SetActive(true);
        if (GameController.Singleton.currentPhase == GamePhase.gameplay)
        {
            this.SetActiveGameplayButtons(true);
        }
    }

    [Client]
    public void OnLocalPlayerTurnEnd()
    {
        //todo : display "Waiting for other player" msg            
        this.endTurnButton.SetActive(false);
        if (GameController.Singleton.currentPhase == GamePhase.gameplay)
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

    private void OnTurnOrderIndexChanged(int newTurnIndex)
    {
        if (newTurnIndex == -1)
            return;

        if (GameController.Singleton.ItsMyTurn())
        {
            this.SetInteractableGameplayButtons(true);
        }
    }
    #endregion

    //used for testing functionnalities without waiting for client setup
    public void TestButton()
    {

    }
}