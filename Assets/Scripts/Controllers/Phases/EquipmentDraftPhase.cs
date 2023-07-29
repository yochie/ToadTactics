using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.equipmentDraft;

    private int startingPlayerID;

    private int remainingCountToDraft;
    private List<string> equipmentIDstoAssign;

    public EquipmentDraftPhase(int startingPlayerID = 0)
    {
        this.startingPlayerID = startingPlayerID;
    }

    public void Init(string name, GameController controller)
    {
        Debug.Log("Initializing equipment draft phase");
        this.Name = name;
        this.Controller = controller;
        this.Controller.playerTurn = this.startingPlayerID;

        uint numToRoll = this.Controller.defaultNumEquipmentsDraftedBetweenRounds;
        this.remainingCountToDraft = (int) numToRoll;
        List<string> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;
            int j = 0;
            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); j++; } while (rolledIDs.Contains(newEquipmentID) && j < 9999);
            rolledIDs.Add(newEquipmentID);
        }

        //will init slots using Rpcs (careful, async, need to set all state before)
        this.equipmentIDstoAssign = rolledIDs;
        this.Controller.equipmentDraftUI.InitSlotsForDraft(rolledIDs);
    }

    public void Tick()
    {
        Controller.SwapPlayerTurn();

        this.remainingCountToDraft--;

        if (this.AllCharactersAreDrafted())
        {
            Debug.Log("All chars drafted. Setting up king selection.");
            //this.Controller.equipmentDraftUI.RpcSetupEquipmentAssignment(equipmentIDstoAssign);
        }
    }

    private bool AllCharactersAreDrafted()
    {
        if (this.remainingCountToDraft <= 0)
            return true;
        else
            return false;
    }
}
