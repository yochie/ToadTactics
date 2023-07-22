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
    private GameObject CharacterSheetsList;

    [SerializeField]
    private List<DraftableCharacterSlotUI> draftableSlots;

    private List<DraftableCharacterSlotUI> kingChoiceSlots;

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
    public void Init(List<int> classIDsToDraft)
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient =  GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (DraftableCharacterSlotUI slot in draftableSlots)
        {
            slot.TargetRpcInit(currentPlayerClient, classIDsToDraft[i], true);
            slot.TargetRpcInit(waitingPlayerClient, classIDsToDraft[i], false);
            i++;
        }
    }

    private DraftableCharacterSlotUI GetSlotForID(int classID)
    {
        foreach (DraftableCharacterSlotUI slot in draftableSlots)
        {
            if (slot.holdsClassID == classID)
                return slot;
        }

        throw new System.Exception("No slot with given ID was found");
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
            GameObject kingCandidateSlotObject = Instantiate(this.slotPrefab, this.CharacterSheetsList.transform);
            DraftableCharacterSlotUI kingCandidateSlot = kingCandidateSlotObject.GetComponent<DraftableCharacterSlotUI>();
            kingCandidateSlot.Init(classID, true);
        }
    }

    private void ClearDraftableSlots()
    {
        foreach (DraftableCharacterSlotUI slot in this.draftableSlots)
        {
            Destroy(slot.gameObject);
        }

    }

    public void OnCharacterCrowned(int classID)
    {
        this.instructionLabel.text = "Waiting for other player to choose his king.";
    }
}
