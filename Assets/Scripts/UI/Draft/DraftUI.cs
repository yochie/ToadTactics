using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;
using System.Linq;

public class DraftUI : NetworkBehaviour
{
    [SerializeField]
    private GameObject slotPrefab;

    [SerializeField]
    private GameObject characterSheetsListFirstRow;
    [SerializeField]
    private GameObject characterSheetsListSecondRow;

    [SerializeField]
    private List<DraftableCharacterPanelUI> draftableSlots;

    [SerializeField]
    private TextMeshProUGUI instructionLabel;
    
    [SerializeField]
    private MessagePopup messagePopup;

    [SerializeField]
    private DiceRollPopup diceRollPopup;

    [SerializeField]
    private TextMeshProUGUI loadingMessage;

    [SerializeField]
    private GameObject draftGrid;
    
    [SerializeField]
    private GameObject ownDraftedList;

    [SerializeField]
    private GameObject opponentDraftedList;

    private void Awake()
    {
        //just to avoid printing error when starting editor in wrong draft scene
        //Gamecontroller should always exist otherwise
        if (GameController.Singleton == null)
            return;
        GameController.Singleton.draftUI = this;
    }

    [ClientRpc]
    public void RpcInitDraft(List<int> classIDsToDraft, int startingPlayerID)
    {
        int i = 0;
        foreach (DraftableCharacterPanelUI slot in draftableSlots)
        {

            slot.Init(classIDsToDraft[i], forKingAssignment:false);

            i++;
        }
        this.loadingMessage.gameObject.SetActive(false);
        this.draftGrid.SetActive(true);
        this.ownDraftedList.SetActive(true);
        this.opponentDraftedList.SetActive(true);
        this.instructionLabel.gameObject.SetActive(true);

        this.diceRollPopup.ShowRollOutcome(GameController.Singleton.LocalPlayer.playerID == startingPlayerID);
    }

    [TargetRpc]
    internal void TargetRpcEnableDraftButtons(NetworkConnectionToClient target)
    {
        foreach (DraftableCharacterPanelUI slot in draftableSlots)
        {
            slot.EnableDraftButton();
        }
    }

    //Called locally whenever a draft button is clicked to prevent additional invalid inputs
    public void DisableDraftButtons()
    {
        foreach (DraftableCharacterPanelUI slot in draftableSlots)
        {
            slot.SetButtonActiveState(enabled: false);
        }
    }

    public void SetupKingSelection(List<int> classIDs)
    {
        StartCoroutine(this.SwitchToKingSelectCoroutine(classIDs));
    }

    private IEnumerator SwitchToKingSelectCoroutine(List<int> classIDs)
    {
        this.messagePopup.TriggerPopup("Draft complete", Color.white);
        yield return new WaitForSeconds(this.messagePopup.GetPopupTotalDuration());
        this.FinishKingSelectionSetup(classIDs);
    }

    private void FinishKingSelectionSetup(List<int> classIDs)
    {
        this.ClearDraftableSlots();
        this.GenerateKingCandidates(classIDs);
        this.characterSheetsListSecondRow.SetActive(false);
        this.instructionLabel.text = "Crown your king";
    }

    private void GenerateKingCandidates(List<int> classIDs)
    {
        foreach (int classID in classIDs)
        {
            GameObject kingCandidateSlotObject = Instantiate(this.slotPrefab, this.characterSheetsListFirstRow.transform);
            DraftableCharacterPanelUI kingCandidateSlot = kingCandidateSlotObject.GetComponent<DraftableCharacterPanelUI>();
            kingCandidateSlot.Init(classID, true);
        }
    }

    private void ClearDraftableSlots()
    {
        foreach (DraftableCharacterPanelUI slot in this.draftableSlots)
        {
            Destroy(slot.gameObject);
        }

    }

    public void OnCharacterCrowned(int classID)
    {
        this.instructionLabel.text = "Waiting for opponent";
    }

    public void OnLocalPlayerTurnStart()
    {
        this.instructionLabel.text = "Pick a character";
    }
    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting for opponent";
    }
}
