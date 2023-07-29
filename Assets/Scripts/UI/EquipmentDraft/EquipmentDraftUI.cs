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
    private GameObject equipmentSlotPrefab;

    [SerializeField]
    private GameObject characterSlotPrefab;

    [field: SerializeField]
    public GameObject EquipmentSheetsList { get; private set; }

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

    [Server]
    public void InitSlotsForDraft(List<string> equipmentIDsToDraft)
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));
        int i = 0;
        foreach (string equipmentID in equipmentIDsToDraft)
        {
            GameObject slotObject = Instantiate(this.equipmentSlotPrefab, this.EquipmentSheetsList.transform);
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

    //[ClientRpc]
    //public void RpcSetupEquipmentAssignment(List<string> equipmentIDs)
    //{
    //    this.ClearDraftableSlots();
    //    List<int> charactersToAssignTo = new();
    //    GameController.Singleton.playerCharacters.Keys.CopyTo(charactersToAssignTo);
    //    int localPlayer = GameController.Singleton.LocalPlayer.playerID;
    //    charactersToAssignTo.Where(characterID => !GameController.Singleton.HeOwnsThisCharacter(localPlayer, ))
    //    this.GenerateEquipmentAssignCandidates(.);
    //    this.currentlyAssigningEquipmentID = equipmentIDs[0];
    //    this.instructionLabel.text = "Assign your equipments";
    //}

    //private void GenerateEquipmentAssignCandidates(List<int> playerCharacters)
    //{
    //    foreach (string equipmentID in equipmentIDs)
    //    {
    //        GameObject slotObject = Instantiate(this.equipmentSlotPrefab, this.EquipmentSheetsList.transform);
    //        DraftableEquipmentSlotUI equipmentSlot = slotObject.GetComponent<DraftableEquipmentSlotUI>();
    //        equipmentSlot.Init(equipmentID: equipmentID, itsYourTurn: true, asAssignmentCandidate: true);
    //    }
    //}

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
