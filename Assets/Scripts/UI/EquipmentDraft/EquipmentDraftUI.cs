using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;

public class EquipmentDraftUI : MonoBehaviour
{
    [SerializeField]
    private GameObject slotPrefab;

    [field: SerializeField]
    public GameObject EquipmentSheetsList { get; private set; }

    [SerializeField]
    private TextMeshProUGUI instructionLabel;

    private List<DraftableEquipmentSlotUI> draftableSlots;

    private void Awake()
    {
        Debug.Log("Awaking equipment draft UI");
        //just to avoid printing error when starting editor in wrong draft scene
        //Gamecontroller should always exist otherwise
        if (GameController.Singleton == null)
            return;
        GameController.Singleton.equipmentDraftUI = this;
    }

    [Server]
    public void InitSlotContents(List<string> equipmentIDsToDraft)
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (string equipmentID in equipmentIDsToDraft)
        {
            GameObject slotObject = Instantiate(this.slotPrefab, this.EquipmentSheetsList.transform);
            DraftableEquipmentSlotUI slot = slotObject.GetComponent<DraftableEquipmentSlotUI>();
            NetworkServer.Spawn(slot.gameObject);
            Debug.Log("Spawned DraftableEquipmentSlot");
            slot.TargetRpcInitForDraft(currentPlayerClient, equipmentIDsToDraft[i], true);
            slot.TargetRpcInitForDraft(waitingPlayerClient, equipmentIDsToDraft[i], false);
            i++;
        }
    }

    public void SetupEquipmentAssigning(List<string> equipmentIDs)
    {
        this.ClearDraftableSlots();
        this.GenerateAssignCandidates(equipmentIDs);
        this.instructionLabel.text = "Assign your equipments";
    }

    private void GenerateAssignCandidates(List<string> equipmentIDs)
    {
        foreach (string equipmentID in equipmentIDs)
        {
            GameObject slotObject = Instantiate(this.slotPrefab, this.EquipmentSheetsList.transform);
            DraftableEquipmentSlotUI equipmentSlot = slotObject.GetComponent<DraftableEquipmentSlotUI>();
            equipmentSlot.Init(equipmentID: equipmentID, itsYourTurn: true, asAssignmentCandidate: true);
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
