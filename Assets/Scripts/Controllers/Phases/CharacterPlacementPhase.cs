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
        this.Controller.SetTreasureOpenedByPlayerID(-1);
        this.Controller.ClearTurnOrder();
        this.Controller.ClearPlayerCharacters();

        this.Controller.SetCurrentRound(this.Controller.CurrentRound + 1);

        Map.Singleton.Initialize();

        //create characters
        foreach (PlayerController player in this.Controller.playerControllers)
        {
            foreach (KeyValuePair<int, int> characterToOwner in this.Controller.DraftedCharacterOwners)
            {
                if (characterToOwner.Value == player.playerID)
                    player.CreateCharacter(characterToOwner.Key);
            }
        }

        this.Controller.StartCoroutine(WaitForCharacterInstantiationThenFinishInit());
    }


    //character init SEEMS to have properly been executed and synced to clients at this point... not sure why
    //Best guess is that OnclientStart always runs on server before client, which results in spawning on client with all setup syncvars
    //syncvars are then all filled in before object is registered as spawned on client
    //TODO : I should probably move some stuff around so that charclass and Init are more clearly setup on server before server spawning
    private void FinishInit()
    {
        //apply passives to characters
        foreach(PlayerCharacter character in this.Controller.PlayerCharactersByID.Values)
        {
            character.ApplyPassiveAbilityBuffs();
        }

        //setup turn order list
        foreach (int classID in this.Controller.DraftedCharacterOwners.Keys)
        {
            this.Controller.AddCharToTurnOrder(classID);
        }

        //DOESN't work because previous lines trigger RPC that resets life labels AFTER following lines
        //Debug.Log("Updating life labels");
        //TurnOrderHUD.Singleton.UpdateLifeLabels();

        //do an artificial setting to trigger new player turn event even if he was last to play during draft
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
            Controller.SetPhase(new GameplayPhase());
        }
    }

    private IEnumerator WaitForCharacterInstantiationThenFinishInit()
    {
        while (!Controller.AllCharactersInstantiatedOnClients())
        {
            yield return null;
        }

        this.FinishInit();
    }
}
