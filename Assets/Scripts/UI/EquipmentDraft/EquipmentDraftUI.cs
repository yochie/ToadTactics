using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;
using System.Linq;

public class EquipmentDraftUI : NetworkBehaviour
{

    [SerializeField]
    private DraftableEquipmentSlotUI draftableEquipmentPanelPrefab;

    [SerializeField]
    private GameObject assignmentEquipmentPanelPrefab;

    [SerializeField]
    private GameObject assignmentCharacterPanelPrefab;

    [SerializeField]
    private GameObject draftEquipmentPanelsFirstRow;

    [SerializeField]
    private GameObject draftEquipmentPanelsSecondRow;

    [SerializeField]
    private EquipmentDraftCharacterListUI ownCharacterList;

    [SerializeField]
    private EquipmentDraftCharacterListUI opponentCharacterList;

    [SerializeField]
    private GameObject draftContainer;

    [field: SerializeField]
    public  GameObject AssignmentEquipmentPanelRow { get; private set; }

    [field: SerializeField]
    public GameObject AssignmentCharacterPanelsRow { get; private set; }

    [SerializeField]
    private GameObject AssignmentContainer;

    [SerializeField]
    private TextMeshProUGUI instructionLabel;

    [SerializeField]
    private MessagePopup messagePopup;

    [SerializeField]
    private GameObject loadingMessage;


    private List<DraftableEquipmentSlotUI> draftableSlots;

    private AssignmentEquipmentPanelUI currentAssigmentEquipmentPanel;
    private List<AssignmentCharacterPanelUI> assignmentCharacterPanels;

    public string currentlyAssigningEquipmentID;

    private void Awake()
    {
        Debug.Log("Awaking equipment draft UI");
        if (GameController.Singleton != null)            
            GameController.Singleton.equipmentDraftUI = this;
    }

    [TargetRpc]
    public void TargetRpcInitForDraft(NetworkConnectionToClient target, List<string> equipmentIDsToDraft, bool youStart, List<int> ownedCharacterIDs, List<int> opponentCharacterIDs)
    {

        this.ownCharacterList.Init(ownedCharacterIDs);
        this.opponentCharacterList.Init(opponentCharacterIDs);
        this.SetInstructionForTurn(yourTurn: youStart);

        int i = 0;
        this.draftableSlots = new();
        foreach (string equipmentID in equipmentIDsToDraft)
        {
            DraftableEquipmentSlotUI slot = Instantiate(this.draftableEquipmentPanelPrefab, this.draftEquipmentPanelsFirstRow.transform);
            GameObject row;
            if (i < GameController.Singleton.numEquipmentsDraftedBetweenRounds / 2)
                row = this.draftEquipmentPanelsFirstRow;
            else
                row = this.draftEquipmentPanelsSecondRow;
            slot.transform.SetParent(row.transform, worldPositionStays: false);
            slot.Init(equipmentID);
            if (youStart)
                slot.SetButtonActiveState(enabled: youStart);
            this.draftableSlots.Add(slot);
            i++;
        }
        this.loadingMessage.SetActive(false);
        this.ownCharacterList.gameObject.SetActive(true);
        this.opponentCharacterList.gameObject.SetActive(true);
        this.instructionLabel.gameObject.SetActive(true);
    }

    [TargetRpc]
    internal void TargetRpcUpdateDraftSlotsForTurn(NetworkConnectionToClient target, bool itsYourTurn, List<string> alreadyDrafted)
    {
        foreach (DraftableEquipmentSlotUI slot in this.draftableSlots)
        {
            bool slotHasBeenDrafted = alreadyDrafted.Contains(slot.holdsEquipmentID);

            if (slotHasBeenDrafted)
                slot.GrayOut();

            if (itsYourTurn && !slotHasBeenDrafted)
                slot.SetButtonActiveState(true);
            else
                slot.SetButtonActiveState(false);
        }
    }

    [TargetRpc]
    public void TargetRpcSetupEquipmentAssignment(NetworkConnectionToClient target, string firstEquipmentID, List<int> teamIDs, List<CharacterStats> teamStats)
    {
        Action afterTransition = () =>
        {
            this.draftContainer.SetActive(false);
            this.AssignmentContainer.SetActive(true);

            this.currentlyAssigningEquipmentID = firstEquipmentID;
            this.instructionLabel.text = "Assign your equipments";

            this.GenerateAssignmentEquipmentPanel(firstEquipmentID);
            Dictionary<int, CharacterStats> teamStatsByID = new();
            int i = 0;
            foreach (int charID in teamIDs)
            {
                teamStatsByID.Add(charID, teamStats[i++]);
            }
            this.GenerateAssignmentCharacterPanels(teamStatsByID);
        };
        StartCoroutine(this.SwitchToAssignmentCoroutine(afterTransition));
    }

    private IEnumerator SwitchToAssignmentCoroutine(Action afterTransition)
    {
        this.messagePopup.TriggerPopup("Draft complete", Color.white);
        yield return new WaitForSeconds(this.messagePopup.GetPopupTotalDuration());
        afterTransition();
    }

    private void GenerateAssignmentEquipmentPanel(string firstEquipmentID)
    {
        GameObject slotObject = Instantiate(this.assignmentEquipmentPanelPrefab, this.AssignmentEquipmentPanelRow.transform);
        AssignmentEquipmentPanelUI assignmentEquipmentPanel = slotObject.GetComponent<AssignmentEquipmentPanelUI>();
        this.currentAssigmentEquipmentPanel = assignmentEquipmentPanel;
        assignmentEquipmentPanel.FillWithEquipmentData(firstEquipmentID);
    }

    private void GenerateAssignmentCharacterPanels(Dictionary<int, CharacterStats> teamStats)
    {
        this.assignmentCharacterPanels = new();
        Dictionary<string, int> localPlayerAssignedEquipments = GameController.Singleton.LocalPlayer.AssignedEquipmentsCopy;
        foreach (int panelForClassID in teamStats.Keys)
        {
            GameObject slotObject = Instantiate(this.assignmentCharacterPanelPrefab, this.AssignmentCharacterPanelsRow.transform);
            AssignmentCharacterPanelUI assignmentCharacterPanel = slotObject.GetComponent<AssignmentCharacterPanelUI>();
            this.assignmentCharacterPanels.Add(assignmentCharacterPanel);
            List<string> previouslyAssignedEquipments = localPlayerAssignedEquipments
                .Where(assignedEquipment => assignedEquipment.Value == panelForClassID)
                .Select(assignedEquipment => assignedEquipment.Key).ToList<string>();
            assignmentCharacterPanel.Init(panelForClassID, previouslyAssignedEquipments, teamStats[panelForClassID], GameController.Singleton.IsAKing(panelForClassID));
        }
    }

    public void OnLocalPlayerAssignedAllEquipments()
    {
        foreach(AssignmentCharacterPanelUI characterSheet in this.assignmentCharacterPanels)
        {
            characterSheet.SetButtonActiveState(false);
        }
        this.instructionLabel.text = "Waiting for opponent";
    }


    [TargetRpc]
    internal void TargetRPCUpdateCharacterStats(NetworkConnectionToClient sender, int classID, CharacterStats currentStats, bool isKing)
    {
        AssignmentCharacterPanelUI panel = this.assignmentCharacterPanels.Single(panel => panel.IsForCharID(classID));
        panel.UpdateStats(currentStats, isKing);
    }
    public void OnLocalPlayerTurnStart()
    {
        this.SetInstructionForTurn(yourTurn: true);
    }

    public void OnLocalPlayerTurnEnd()
    {
        this.SetInstructionForTurn(yourTurn: false);
    }

    private void SetInstructionForTurn(bool yourTurn)
    {
        this.instructionLabel.text = yourTurn ? "Pick an equipment" : "Waiting for opponent";
    }

    [TargetRpc]
    internal void TargetRpcUpdateEquipmentAssignment(NetworkConnectionToClient target, string nextEquipmentID)
    {
        //Debug.LogFormat("Rpc for updating equipment slot with data for {0}", nextEquipmentID);

        this.currentlyAssigningEquipmentID = nextEquipmentID;
        this.currentAssigmentEquipmentPanel.FillWithEquipmentData(nextEquipmentID);
    }
}