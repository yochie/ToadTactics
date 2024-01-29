using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class CharacterDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.characterDraft;

    [Server]
    public void Init(string name, GameController controller)
    {
        this.Name = name;
        this.Controller = controller;


        uint numToRoll = this.Controller.defaultNumCharsPerPlayer * 2;
        List<int> rolledIDs = new();
        for (int i = 0; i < numToRoll; i++)
        {
            int newClassID;
            do { newClassID = ClassDataSO.Singleton.GetRandomClassID(); } while (rolledIDs.Contains(newClassID));
            rolledIDs.Add(newClassID);
        }

        //will init slots using Rpcs (careful, async, need to set all state before)
        int startingPlayerID = UnityEngine.Random.Range(0, Utility.NUM_PLAYERS);

        this.Controller.draftUI.RpcInitDraft(rolledIDs, startingPlayerID);

        this.Controller.StartCoroutine(WaitForDiceThenRollStartDraftCoroutine(startingPlayerID));
    }

    private IEnumerator WaitForDiceThenRollStartDraftCoroutine(int startingPlayerID)
    {
        while (this.Controller.DiceRollPopupPlayedOnClients < Utility.NUM_PLAYERS)
            yield return null;

        NetworkConnectionToClient startingPlayerConnection = GameController.Singleton.GetConnectionForPlayerID(startingPlayerID);
        this.Controller.draftUI.TargetRpcEnableDraftButtons(startingPlayerConnection);
        this.Controller.SetPlayerTurn(startingPlayerID);
    }

    [Server]
    public void Tick()
    {
        if (!this.Controller.AllCharactersDrafted())
        {
            Controller.SwapPlayerTurn();

        } else
        {
            Debug.Log("All chars drafted. Setting up king selection.");
            this.Controller.RpcSetupKingSelection();
        }
    }
}
