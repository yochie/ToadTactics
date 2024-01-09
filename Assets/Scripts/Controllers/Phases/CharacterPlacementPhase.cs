using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

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
    private void InitPostCharacterCreation()
    {
        //apply passives to characters, needs to be called here instead of character init because relies on list of playerCharacters on controller
        foreach (PlayerCharacter character in this.Controller.PlayerCharactersByID.Values)
        {
            character.ApplyAbilityBuffsForRoundStart();
        }

        //setup turn order list
        foreach (PlayerCharacter character in this.Controller.PlayerCharactersByID.Values)
        {
            this.Controller.AddCharToTurnOrder(character.CharClassID);
        }

        List<TurnOrderSlotInitData> slotDataList = new();
        foreach (PlayerCharacter character in this.Controller.PlayerCharactersByID.Values)
        {
            bool isAKing = GameController.Singleton.IsAKing(character.CharClassID);
            bool itsHisTurn = GameController.Singleton.ItsThisCharactersTurn(character.CharClassID);
            int maxHealth = character.CurrentStats.maxHealth;
            Dictionary<int, string> characterBuffDataIDs = character.GetDisplayedAffectingBuffDataIDs();
            Dictionary<int, int> buffDurationsByUniqueID = new();
            foreach (RuntimeBuff buff in character.affectedByBuffs)
            {
                RuntimeBuffTimeout timerComponent = buff.GetComponent<RuntimeBuffTimeout>();
                int remainingDuration = -1;
                if (timerComponent != null)
                    remainingDuration = timerComponent.TurnDurationRemaining;
                buffDurationsByUniqueID.Add(buff.UniqueID, remainingDuration);
            }
            TurnOrderSlotInitData slotData = new(character.CharClassID, isAKing, itsHisTurn, maxHealth, characterBuffDataIDs, buffDurationsByUniqueID);
            slotDataList.Add(slotData);
        }

        this.Controller.RpcInitTurnOrderHud(slotDataList, this.Controller.SortedTurnOrder.Values.ToList());

        foreach(PlayerController player in Controller.playerControllers)
        {
            List<int> ownedCharacterIDs = this.Controller.GetCharacterIDsOwnedByPlayer(player.playerID);
            List<int> opponentCharacterIDs = this.Controller.GetCharacterIDsOwnedByPlayer(this.Controller.OtherPlayer(player.playerID));
            player.TargetRpcInitOwnCharacterSlotsList(ownedCharacterIDs);
            player.TargetRpcInitOpponentCharacterSlotsList(opponentCharacterIDs);
            //TODO: change once starting player is actually random
            bool isStartingPlayer = player.playerID == 0 ? true : false;
            MainHUD.Singleton.TargetRpcEnablePlacementInstruction(player.connectionToClient, yourTurn: isStartingPlayer);
        }

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

        this.InitPostCharacterCreation();
    }
}
