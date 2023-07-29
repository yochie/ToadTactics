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
    private GameObject draftableEquipmentSlotPrefab;

    [SerializeField]
    private GameObject assignmentEquipmentSheetPrefab;

    [SerializeField]
    private GameObject assignmentCharacterSheetPrefab;

    [field: SerializeField]
    public GameObject DraftEquipmentSheetsList { get; private set; }

    [field: SerializeField]
    public  GameObject AssignmentEquipmentSheetContainer { get; private set; }

    [field: SerializeField]
    public GameObject AssignmentCharacterSheets { get; private set; }

    [SerializeField]
    private TextMeshProUGUI instructionLabel;

    private List<DraftableEquipmentSlotUI> draftableSlots;

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
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (string equipmentID in equipmentIDsToDraft)
        {
            GameObject slotObject = Instantiate(this.draftableEquipmentSlotPrefab, this.DraftEquipmentSheetsList.transform);
            DraftableEquipmentSlotUI slot = slotObject.GetComponent<DraftableEquipmentSlotUI>();
            NetworkServer.Spawn(slot.gameObject);
            //Debug.Log("Spawned DraftableEquipmentSlot");
            slot.TargetRpcInitForDraft(target: currentPlayerClient, equipmentID: equipmentIDsToDraft[i], itsYourTurn: true);
            slot.TargetRpcInitForDraft(target: waitingPlayerClient, equipmentID: equipmentIDsToDraft[i], itsYourTurn: false);
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
    public void TargetRpcSetupEquipmentAssignment(NetworkConnectionToClient target, string firstEquipmentID, List<int> charactersToAssignTo)
    {
        Destroy(this.DraftEquipmentSheetsList);
        this.AssignmentCharacterSheets.SetActive(true);
        this.AssignmentEquipmentSheetContainer.SetActive(true);
        this.ClearDraftableSlots();
        
        this.currentlyAssigningEquipmentID = firstEquipmentID;
        this.instructionLabel.text = "Assign your equipments";

        this.GenerateAssignmentEquipmentSheet(firstEquipmentID);
        this.GenerateAssignmentCharacterSheets(charactersToAssignTo);
    }

    private void GenerateAssignmentEquipmentSheet(string firstEquipmentID)
    {
        GameObject slotObject = Instantiate(this.assignmentEquipmentSheetPrefab, this.AssignmentEquipmentSheetContainer.transform);
        AssignmentEquipmentSheetUI assignmentEquipmentSheet = slotObject.GetComponent<AssignmentEquipmentSheetUI>();
        assignmentEquipmentSheet.Init(firstEquipmentID);
    }

    private void GenerateAssignmentCharacterSheets(List<int> classIDs)
    {
        foreach (int classID in classIDs)
        {
            GameObject slotObject = Instantiate(this.assignmentCharacterSheetPrefab, this.AssignmentCharacterSheets.transform);
            AssignmentCharacterSheetUI assignmentCharacterSheet = slotObject.GetComponent<AssignmentCharacterSheetUI>();
            assignmentCharacterSheet.Init(classID);
        }
    }

    private void ClearDraftableSlots()
    {
        foreach (DraftableEquipmentSlotUI slot in this.draftableSlots)
        {
            Destroy(slot.gameObject);
        }
    }

    public void OnLocalPlayerAssignedAllEquipments()
    {
        this.instructionLabel.text = "Waiting for other player to assign their equipments";
    }

    public void OnLocalPlayerTurnStart()
    {
        this.instructionLabel.text = "Pick an equipment";
    }
    public void OnLocalPlayerTurnEnd()
    {
        this.instructionLabel.text = "Waiting for other player to choose";
    }
}