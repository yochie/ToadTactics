using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.equipmentDraft;

    private int startingPlayerID;

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
        List<string> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            string newEquipmentID;
            int j = 0;
            do { newEquipmentID = EquipmentDataSO.Singleton.GetRandomEquipmentID(); j++; } while (rolledIDs.Contains(newEquipmentID) && j < 9999);
            rolledIDs.Add(newEquipmentID);
        }

        //will init slots using Rpcs (careful, async, need to set all state before)
        this.Controller.equipmentDraftUI.InitSlotContents(rolledIDs);
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }
}
