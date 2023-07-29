using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;

public class DraftUI : MonoBehaviour
{
    [SerializeField]
    private GameObject slotPrefab;

    [SerializeField]
    private GameObject characterSheetsList;

    [SerializeField]
    private List<AssignmentCharacterSlotUI> draftableSlots;

    [SerializeField]
    private TextMeshProUGUI instructionLabel;

    private void Awake()
    {
        //just to avoid printing error when starting editor in wrong draft scene
        //Gamecontroller should always exist otherwise
        if (GameController.Singleton == null)
            return;
        GameController.Singleton.draftUI = this;
    }

    [Server]
    public void InitSlotContents(List<int> classIDsToDraft)
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient =  GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (AssignmentCharacterSlotUI slot in draftableSlots)
        {
            slot.TargetRpcInitForDraft(currentPlayerClient, classIDsToDraft[i], true);
            slot.TargetRpcInitForDraft(waitingPlayerClient, classIDsToDraft[i], false);
            i++;
        }
    }

    public void SetupKingSelection(List<int> classIDs)
    {
        this.ClearDraftableSlots();
        this.GenerateKingCandidates(classIDs);
        this.instructionLabel.text = "Crown your king";
    }

    private void GenerateKingCandidates(List<int> classIDs)
    {
        foreach (int classID in classIDs)
        {
            GameObject kingCandidateSlotObject = Instantiate(this.slotPrefab, this.characterSheetsList.transform);
            AssignmentCharacterSlotUI kingCandidateSlot = kingCandidateSlotObject.GetComponent<AssignmentCharacterSlotUI>();
            kingCandidateSlot.Init(classID, true, true);
        }
    }

    private void ClearDraftableSlots()
    {
        foreach (AssignmentCharacterSlotUI slot in this.draftableSlots)
        {
            Destroy(slot.gameObject);
        }

    }

    public void OnCharacterCrowned(int classID)
    {
        this.instructionLabel.text = "Waiting for other player to choose his king";
    }

    public void OnLocalPlayerTurnStart()
    {
        this.instructionLabel.text = "Pick a character";
    }
    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting for other player to choose";
    }
}
