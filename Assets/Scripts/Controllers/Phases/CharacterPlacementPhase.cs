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

        this.Name = name;
        this.Controller = controller;

        Controller.playerTurn = 0;
        Controller.inputHandler.SetControlModeOnAllClients(ControlMode.characterPlacement);
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
