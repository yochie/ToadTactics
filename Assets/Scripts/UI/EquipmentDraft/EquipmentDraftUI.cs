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
    private GameObject draftableEquipmentPanelPrefab;

    [SerializeField]
    private GameObject assignmentEquipmentPanelPrefab;

    [SerializeField]
    private GameObject assignmentCharacterPanelPrefab;

    [field: SerializeField]
    public GameObject DraftEquipmentPanelsFirstRow { get; private set; }

    [field: SerializeField]
    public GameObject DraftEquipmentPanelsSecondRow { get; private set; }

    [SerializeField]
    private GameObject DraftContainer;

    [field: SerializeField]
    public  GameObject AssignmentEquipmentPanelRow { get; private set; }

    [field: SerializeField]
    public GameObject AssignmentCharacterPanelsRow { get; private set; }

    [SerializeField]
    private GameObject AssignmentContainer;

    [SerializeField]
    private TextMeshProUGUI instructionLabel;

    private List<DraftableEquipmentSlotUI> draftableSlots;

    private AssignmentEquipmentPanelUI currentAssigmentEquipmentPanel;
    private List<AssignmentCharacterPanelUI> assignmentCharacterPanels;

    public string currentlyAssigningEquipmentID;

    private void Awake()
    {
        Debug.Log("Awaking equipment draft UI");
        //just to avoid printing error when starting editor in wrong draft scene
        //Gamecontroller should always exist otherwise
        if (GameController.Singleton == null)
            return;
        GameController.Singleton.equipmentDraftUI = this;
    }

    //TODO: move net code to phase object, keep this UI only as RPC
    [Server]
    public void InitSlotsForDraft(List<string> equipmentIDsToDraft)
    {
        int currentPlayerID = GameController.Singleton.PlayerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (string equipmentID in equipmentIDsToDraft)
        {
            GameObject slotObject = Instantiate(this.draftableEquipmentPanelPrefab, this.DraftEquipmentPanelsFirstRow.transform);
            DraftableEquipmentSlotUI slot = slotObject.GetComponent<DraftableEquipmentSlotUI>();
            NetworkServer.Spawn(slot.gameObject);
            //Debug.Log("Spawned DraftableEquipmentSlot");
            slot.TargetRpcInitForDraft(target: currentPlayerClient, equipmentID: equipmentIDsToDraft[i], itsYourTurn: true, index: i);
            slot.TargetRpcInitForDraft(target: waitingPlayerClient, equipmentID: equipmentIDsToDraft[i], itsYourTurn: false, index: i);
            i++;
        }
    }

    //called by slots in their init
    internal void RegisterSpawnedSlot(DraftableEquipmentSlotUI slot)
    {
        if (this.draftableSlots == null)
            this.draftableSlots = new();
        this.draftableSlots.Add(slot);
    }

    [TargetRpc]
    internal void TargetRpcUpdateDraftSlotsForTurn(NetworkConnectionToClient target, bool itsYourTurn, List<string> alreadyDrafted)
    {
        foreach(DraftableEquipmentSlotUI slot in this.draftableSlots)
        {
            if (itsYourTurn && !alreadyDrafted.Contains(slot.holdsEquipmentID))
                slot.SetButtonActiveState(true);
            else
                slot.SetButtonActiveState(false);
        }
    }

    [TargetRpc]
    public void TargetRpcSetupEquipmentAssignment(NetworkConnectionToClient target, string firstEquipmentID, List<int> teamIDs, List<CharacterStats> teamStats)
    {
        this.DraftContainer.SetActive(false);
        this.AssignmentContainer.SetActive(true);
        
        this.currentlyAssigningEquipmentID = firstEquipmentID;
        this.instructionLabel.text = "Assign your equipments";

        this.GenerateAssignmentEquipmentPanel(firstEquipmentID);
        Dictionary<int, CharacterStats> teamStatsByID = new();
        int i = 0;
        foreach(int charID in teamIDs)
        {
            teamStatsByID.Add(charID, teamStats[i++]);
        }
        this.GenerateAssignmentCharacterPanels(teamStatsByID);
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

    public void OnLocalPlayerTurnStart()
    {
        this.instructionLabel.text = "Pick an equipment";
    }

    [TargetRpc]
    internal void TargetRPCUpdateCharacterStats(NetworkConnectionToClient sender, int classID, CharacterStats currentStats, bool isKing)
    {
        AssignmentCharacterPanelUI panel = this.assignmentCharacterPanels.Single(panel => panel.IsForCharID(classID));
        panel.UpdateStats(currentStats, isKing);
    }

    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting for opponent";
    }

    [TargetRpc]
    internal void TargetRpcUpdateEquipmentAssignment(NetworkConnectionToClient target, string nextEquipmentID)
    {
        //Debug.LogFormat("Rpc for updating equipment slot with data for {0}", nextEquipmentID);

        this.currentlyAssigningEquipmentID = nextEquipmentID;
        this.currentAssigmentEquipmentPanel.FillWithEquipmentData(nextEquipmentID);
    }
}