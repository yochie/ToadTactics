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
        Debug.Log(controller.draftedCharacterOwners);
        Debug.Log(controller.mapInputHandler);
        Debug.Log(Map.Singleton);
        this.Name = name;
        this.Controller = controller;

        Map.Singleton.Initialize();

        //setup turn order list
        foreach (int classID in this.Controller.draftedCharacterOwners.Keys)
        {
            this.Controller.AddCharToTurnOrder(classID);
        }

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
