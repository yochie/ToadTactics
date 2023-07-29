using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EquipmentDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.equipmentDraft;

    private bool doneDrafting;
    private int startingPlayerID;

    public EquipmentDraftPhase(int startingPlayerID = 0)
    {
        this.startingPlayerID = startingPlayerID;
    }

    [Server]
    public void Init(string name, GameController controller)
    {
        Debug.Log("Initializing equipment draft phase");
        this.Name = name;
        this.Controller = controller;
        this.Controller.playerTurn = this.startingPlayerID;

        uint numToRoll = this.Controller.numEquipmentsDraftedBetweenRounds;
        List<string> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;
            if (EquipmentDataSO.Singleton.GetEquipmentIDs().Count < numToRoll)
                throw new System.Exception("Not enough equipments available for draft... fix");
            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); } while (rolledIDs.Contains(newEquipmentID));
            rolledIDs.Add(newEquipmentID);
        }

        this.Controller.SetEquipmentsToDraft(rolledIDs);
        this.Controller.SetEquipmentsToAssign(rolledIDs);
        this.doneDrafting = false;

        //will init slots using Rpcs (careful, async, need to set all state before)
        this.Controller.equipmentDraftUI.InitSlotsForDraft(rolledIDs);
    }

    [Server]
    public void Tick()
    {
        this.Controller.SwapPlayerTurn();

        this.UpdateDraftUI();

        if (!this.doneDrafting && this.Controller.AllEquipmentsDrafted())
        {
            Debug.Log("All equipments drafted. Setting up equipment assigning.");
            this.doneDrafting = true;
            //this.Controller.equipmentDraftUI.RpcSetupEquipmentAssignment(equipmentIDstoAssign);
        }
        else if (doneDrafting && this.Controller.AllEquipmentsAssigned())
        {
            Debug.Log("All equipments assigned. Starting new round.");
            this.Controller.CmdChangeToScene("MainGame");
        }
    }

    [Server]
    private void UpdateDraftUI()
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));

        List<string> allDraftedEquipments = new();
        foreach (PlayerController pc in this.Controller.playerControllers)
        {
            foreach (string equipID in pc.GetDraftedEquipmentIDs())
            {
                allDraftedEquipments.Add(equipID);
            }
                
        }

        this.Controller.equipmentDraftUI.TargetRpcUpdateDraftSlotsForTurn(target: currentPlayerClient, itsYourTurn: true, alreadyDrafted: allDraftedEquipments);
        this.Controller.equipmentDraftUI.TargetRpcUpdateDraftSlotsForTurn(target: waitingPlayerClient, itsYourTurn: false, alreadyDrafted: allDraftedEquipments);
    }
}