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

    private bool assigningEquipments;
    private int startingPlayerID;
    private readonly List<string> equipmentsToDraft = new();
    private readonly List<string> equipmentsToAssign = new();
    private string rolledEquipmentIDForTreasureOpener;
    private bool treasureWasOpened;
    private int treasureWasOpenedByPlayerID;


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
        this.Controller.SetPlayerTurn(this.startingPlayerID);


        this.treasureWasOpenedByPlayerID = this.Controller.GetTreasureOpenedByPlayerID();
        this.treasureWasOpened = (this.treasureWasOpenedByPlayerID != -1);
        uint numToRoll = this.Controller.numEquipmentsDraftedBetweenRounds;
        if (this.treasureWasOpened)
            numToRoll++;
        int previouslyDraftedCount = this.Controller.AlreadyDraftedEquipmentCount();
        if (EquipmentDataSO.Singleton.GetEquipmentIDs().Count < numToRoll + previouslyDraftedCount)
            throw new System.Exception("Not enough equipments available for draft... fix");

        List<string> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;

            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); } while (this.Controller.AlreadyDraftedEquipmentID(newEquipmentID));
            rolledIDs.Add(newEquipmentID);
            this.Controller.AddAlreadyDraftedEquipmentID(newEquipmentID);
        }

        if (treasureWasOpened)
        {
            this.SetEquipmentsToAssign(rolledIDs);
            this.rolledEquipmentIDForTreasureOpener = rolledIDs[0];
            rolledIDs.RemoveAt(0);
            this.SetEquipmentsToDraft(rolledIDs);
        }
        else
        {
            this.SetEquipmentsToAssign(rolledIDs);
            this.SetEquipmentsToDraft(rolledIDs);
        }
        this.assigningEquipments = false;

        //will init slots using Rpcs (careful, async, need to set all state before)
        this.Controller.equipmentDraftUI.InitSlotsForDraft(rolledIDs);
    }

    [Server]
    public void Tick()
    {
        if (!this.assigningEquipments && !this.AllEquipmentsDrafted())
        {
            //Debug.Log("Drafting equipment turn swap.");

            this.Controller.SwapPlayerTurn();

            this.UpdateDraftUI();
        }
        else if (!this.assigningEquipments && this.AllEquipmentsDrafted())
        {
            Debug.Log("All equipments drafted. Setting up equipment assigning.");
            this.assigningEquipments = true;

            this.SetupEquipmentAssignment();
        }
        else if (assigningEquipments && this.AllEquipmentsAssigned())
        {
            //Should be called once both players tick after havin assigned all their own equipments
            Debug.Log("All equipments assigned. Starting new round.");
            this.Controller.CmdChangeToScene("MainGame");
        }
    }


    [Server]
    internal void SetEquipmentsToDraft(List<string> equipmentIDs)
    {
        this.equipmentsToDraft.Clear();
        foreach (string equipmentToAdd in equipmentIDs)
        {
            this.equipmentsToDraft.Add(equipmentToAdd);
        }
    }

    [Server]
    private void SetupEquipmentAssignment()
    {
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            if (this.treasureWasOpened && this.treasureWasOpenedByPlayerID == player.playerID)
                player.AddEquipmentIDToAssign(this.rolledEquipmentIDForTreasureOpener);
            NetworkConnectionToClient client = GameController.Singleton.GetConnectionForPlayerID(player.playerID);
            string firstEquipmentToAssign = player.GetUnassignedEquipmentID();
            List<int> characterIDs = new();
            this.Controller.DraftedCharacterOwners.Keys.CopyTo(characterIDs);
            List<int> characterIDsForPlayer = characterIDs.Where(characterID => GameController.Singleton.HeOwnsThisCharacter(player.playerID, characterID)).ToList();

            this.Controller.equipmentDraftUI.TargetRpcSetupEquipmentAssignment(client, firstEquipmentToAssign, characterIDsForPlayer);
        }
    }

    [Server]
    private void UpdateDraftUI()
    {
        int currentPlayerID = GameController.Singleton.PlayerTurn;
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

    [Server]
    internal List<string> GetEquipmentsToDraft()
    {
        List<string> toReturn = new();
        this.equipmentsToDraft.CopyTo(toReturn);
        return toReturn;
    }

    [Server]
    internal void SetEquipmentsToAssign(List<string> equipmentIDs)
    {
        this.equipmentsToAssign.Clear();
        foreach (string equipmentToAdd in equipmentIDs)
        {
            this.equipmentsToAssign.Add(equipmentToAdd);
        }
    }

    [Server]
    public bool AllEquipmentsDrafted()
    {
        foreach (string equipmentIDToDraft in this.equipmentsToDraft)
        {
            bool hasBeenDraftedByAPlayer = false;
            foreach (PlayerController player in this.Controller.playerControllers)
            {
                if (player.HasDraftedEquipment(equipmentIDToDraft))
                    hasBeenDraftedByAPlayer = true;
            }
            if (!hasBeenDraftedByAPlayer)
                return false;
        }

        return true;
    }

    [Server]
    public bool EquipmentHasBeenDrafted(string equipmentID)
    {
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            if (player.HasDraftedEquipment(equipmentID))
                return true;
        }
        return false;
    }

    [Server]
    public bool AllEquipmentsAssigned()
    {
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            if (!player.HasAssignedAllEquipments())
                return false;
        }
        return true;
    }

    [Server]
    public bool EquipmentHasBeenAssigned(string equipmentID)
    {
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            if (player.HasAssignedEquipment(equipmentID))
                return true;
        }
        return false;
    }
}