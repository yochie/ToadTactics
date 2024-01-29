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
    private string treasureToAssign;
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

        GameController.Singleton.RpcLoopMenuSongs();

        this.treasureWasOpenedByPlayerID = this.Controller.GetTreasureOpenedByPlayerID();
        this.treasureWasOpened = (this.treasureWasOpenedByPlayerID != -1);
        if (treasureWasOpened)
        {
            this.treasureToAssign = this.Controller.GetTreasureIDForRound();
            this.Controller.AddAlreadyDraftedEquipmentID(treasureToAssign);
        }
        List<string> rolledIDs = this.Controller.RollNewEquipmentIDs(this.Controller.numEquipmentsDraftedBetweenRounds);

        this.CopyEquipmentsToDraftFrom(rolledIDs);
        this.CopyEquipmentsToAssignFrom(rolledIDs);
        if (treasureWasOpened)
            this.equipmentsToAssign.Add(this.treasureToAssign);
       
        this.assigningEquipments = false;

        //will init slots using Rpcs (careful, async, need to set all state before)
        this.Controller.equipmentDraftUI.InitSlotsForDraft(rolledIDs);

        //create characters anew to display correct stats
        this.Controller.ClearPlayerCharacters();
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            foreach (KeyValuePair<int, int> characterToOwner in this.Controller.DraftedCharacterOwners)
            {
                if (characterToOwner.Value == player.playerID)
                    player.CreateCharacter(characterToOwner.Key, withMap: false);
            }
        }

        foreach (PlayerController player in Controller.playerControllers)
        {
            List<int> ownedCharacterIDs = this.Controller.GetCharacterIDsOwnedByPlayer(player.playerID);
            List<int> opponentCharacterIDs = this.Controller.GetCharacterIDsOwnedByPlayer(this.Controller.OtherPlayer(player.playerID));
            player.TargetRpcInitOwnCharacterSlotsList(ownedCharacterIDs);
            player.TargetRpcInitOpponentCharacterSlotsList(opponentCharacterIDs);
        }
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
        else if (this.assigningEquipments && this.AllEquipmentsAssigned())
        {
            //Should be called once both players tick after havin assigned all their own equipments
            Debug.Log("All equipments assigned. Starting new round.");
            this.Controller.ChangeToScene("MainGame");
        }
    }


    [Server]
    internal void CopyEquipmentsToDraftFrom(List<string> equipmentIDs)
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
                player.AddEquipmentIDToAssign(this.treasureToAssign);
            NetworkConnectionToClient client = GameController.Singleton.GetConnectionForPlayerID(player.playerID);
            string firstEquipmentToAssign = player.GetUnassignedEquipmentID();
            List<int> characterIDs = new();
            this.Controller.DraftedCharacterOwners.Keys.CopyTo(characterIDs);
            List<int> characterIDsForTeam = characterIDs.Where(characterID => GameController.Singleton.HeOwnsThisCharacter(player.playerID, characterID)).ToList();
            List<CharacterStats> statsForTeam = new();
            foreach(int charID in characterIDsForTeam)
            {
                statsForTeam.Add(this.Controller.PlayerCharactersByID[charID].CurrentStats);
            }
            this.Controller.equipmentDraftUI.TargetRpcSetupEquipmentAssignment(client, firstEquipmentToAssign, characterIDsForTeam, statsForTeam);
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
    internal void CopyEquipmentsToAssignFrom(List<string> equipmentIDs)
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