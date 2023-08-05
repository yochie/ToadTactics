using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

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
        this.Controller.SetTurnOrderIndex(-1);
        this.Controller.ClearTurnOrder();
        this.Controller.ClearPlayerCharacters();

        this.Controller.SetCurrentRound(this.Controller.CurrentRound + 1);

        Map.Singleton.Initialize();

        //TODO: create playerCharacters here instead of on placement
        //placement should only make it visible and change its position
        //we probably also want to wait for creation on all clients before proceeding to generate UI and allow character placement

        //setup turn order list
        foreach (int classID in this.Controller.DraftedCharacterOwners.Keys)
        {
            this.Controller.AddCharToTurnOrder(classID);
        }

        //do a false setting to trigger new player turn event even if he was last to play during draft
        //todo : avoid need for this somehow when i have more brainpower left
        this.Controller.SetPlayerTurn(-1);
        this.Controller.SetPlayerTurn(0);

        this.Controller.mapInputHandler.RpcSetControlModeOnAllClients(ControlMode.characterPlacement);
    }

    [Server]
    public void Tick()
    {
        if (!Controller.AllHisCharactersAreOnBoard(Controller.OtherPlayer(Controller.PlayerTurn)))
        {
            Controller.SwapPlayerTurn();
        }

        if (Controller.AllCharactersPlaced())
        {
            this.Controller.StartCoroutine(WaitForAllCharactersInstantiatedOnAllClients());
        }
    }

    private IEnumerator WaitForAllCharactersInstantiatedOnAllClients()
    {
        while (!Controller.AllCharactersInstantiatedOnClients())
        {
            yield return null;
        }
        Controller.SetPhase(new GameplayPhase());
    }
}
