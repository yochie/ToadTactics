using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

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

        //checks for equipments drafted during previous rounds
        int previouslyDrafted = GameController.Singleton.GetEquipmentsToDraft().Count;
        if (EquipmentDataSO.Singleton.GetEquipmentIDs().Count - previouslyDrafted < numToRoll)
            throw new System.Exception("Not enough equipments available for draft... fix");

        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;

            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); } while (rolledIDs.Contains(newEquipmentID) || GameController.Singleton.EquipmentHasBeenDrafted(newEquipmentID));
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
        if (!this.doneDrafting && !this.Controller.AllEquipmentsDrafted())
        {
            //Debug.Log("Drafting equipment turn swap.");

            this.Controller.SwapPlayerTurn();

            this.UpdateDraftUI();
        }
        else if (!this.doneDrafting && this.Controller.AllEquipmentsDrafted())
        {
            Debug.Log("All equipments drafted. Setting up equipment assigning.");
            this.doneDrafting = true;

            this.SetupEquipmentAssignment();
        }
        else if (doneDrafting && this.Controller.AllEquipmentsAssigned())
        {
            //Should be called once both players tick after havin assigned all their own equipments
            Debug.Log("All equipments assigned. Starting new round.");
            this.Controller.CmdChangeToScene("MainGame");
        }
    }

    private void SetupEquipmentAssignment()
    {
        int currentPlayerID = GameController.Singleton.playerTurn;
        NetworkConnectionToClient currentPlayerClient = GameController.Singleton.GetConnectionForPlayerID(currentPlayerID);
        NetworkConnectionToClient waitingPlayerClient = GameController.Singleton.GetConnectionForPlayerID(GameController.Singleton.OtherPlayer(currentPlayerID));

        foreach (PlayerController pc in this.Controller.playerControllers)
        {
            NetworkConnectionToClient client = GameController.Singleton.GetConnectionForPlayerID(pc.playerID);
            string firstEquipmentToAssign = pc.GetUnassignedEquipmentID();
            List<int> characterIDs = new();
            this.Controller.draftedCharacterOwners.Keys.CopyTo(characterIDs);
            List<int> characterIDsForPlayer = characterIDs.Where(characterID => GameController.Singleton.HeOwnsThisCharacter(pc.playerID, characterID)).ToList();

            this.Controller.equipmentDraftUI.TargetRpcSetupEquipmentAssignment(client, firstEquipmentToAssign, characterIDsForPlayer);
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