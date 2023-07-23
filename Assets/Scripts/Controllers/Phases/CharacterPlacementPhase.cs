using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterPlacementPhase : IGamePhase
{
    public GameController Controller { get; set; }
    public string Name { get; set; }
    public GamePhaseID ID => GamePhaseID.characterPlacement;

    [Server]
    public void Init(string name, GameController controller)
    {
        Debug.Log("Initializing character placement mode");
        //Debug.Log(controller.draftedCharacterOwners);
        //Debug.Log(controller.mapInputHandler);
        //Debug.Log(Map.Singleton);
        this.Name = name;
        this.Controller = controller;

        //Clear all state from any previous iterations of this phase
        this.Controller.turnOrderIndex = 0;
        this.Controller.sortedTurnOrder.Clear();
        this.Controller.playerCharactersNetIDs.Clear();
        this.Controller.playerCharacters.Clear();

        this.Controller.currentRound++;

        Map.Singleton.Initialize();

        //setup turn order list
        foreach (int classID in this.Controller.draftedCharacterOwners.Keys)
        {
            this.Controller.AddCharToTurnOrder(classID);
        }

        //do a false setting to trigger new player turn event even if he was last to play during draft
        //todo : avoid need for this somehow when i have more brainpower left
        this.Controller.playerTurn = -1;
        this.Controller.playerTurn = 0;

        this.Controller.mapInputHandler.RpcSetControlModeOnAllClients(ControlMode.characterPlacement);
    }

    [Server]
    public void Tick()
    {
        if (!Controller.AllHisCharactersAreOnBoard(Controller.OtherPlayer(Controller.playerTurn)))
        {
            Controller.SwapPlayerTurn();
        }

        if (Controller.AllPlayerCharactersCreated())
        {
            Controller.SetPhase(new GameplayPhase());
        }
    }
}
