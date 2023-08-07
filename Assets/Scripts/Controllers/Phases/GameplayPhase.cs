using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameplayPhase : IGamePhase
{
    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.gameplay;

    public GameController Controller { get; set; }

    [Server]
    public void Init(string name, GameController controller)
    {
        Debug.Log("Initializing gameplay mode");

        this.Name = name;
        this.Controller = controller;

        Controller.SetTurnOrderIndex(0);
        Controller.SetPlayerTurn(0);

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = -1;
        int i = 0;
        foreach (int classID in Controller.SortedTurnOrder.Values)
        {
            if (i == Controller.TurnOrderIndex)
            {
                currentCharacterClassID = classID;
            }
            i++;
        }
        if (currentCharacterClassID == -1)
        {
            Debug.Log("Error : Bad code for iterating turn order");
        }

        //if we don't own that char, swap player turn
        if (Controller.PlayerTurn != Controller.DraftedCharacterOwners[currentCharacterClassID])
        {
            Controller.SwapPlayerTurn();
        }

        Controller.AssignControlModesForNewTurn(Controller.PlayerTurn, ControlMode.move);
        Controller.RpcOnInitGameplayMode();
    }

    [Server]
    public void Tick()
    {

        int lastTurnCharacterID = this.Controller.GetCharacterIDForTurn();
        if (lastTurnCharacterID == -1)
        {
            throw new System.Exception("Error : couldn't find playing character in turn order");
        }
        BuffManager.Singleton.TickBuffsForTurn(lastTurnCharacterID);
        this.Controller.PlayerCharactersByID[lastTurnCharacterID].TickCooldownsForTurn();

        //loops through turn order                
        if (this.Controller.TurnOrderIndex >= this.Controller.SortedTurnOrder.Count - 1)
            this.Controller.SetTurnOrderIndex(0);
        else
            this.Controller.SetTurnOrderIndex(this.Controller.TurnOrderIndex + 1);

        //finds character class id for the next turn so that we can check who owns it
        
        int newTurnCharacterID = this.Controller.GetCharacterIDForTurn();
        if (newTurnCharacterID == -1)
        {
            throw new System.Exception("Error : couldn't find playing character in turn order");
        }

        PlayerCharacter currentCharacter = this.Controller.PlayerCharactersByID[newTurnCharacterID];
        if (currentCharacter.IsDead || !currentCharacter.CanTakeTurns)
        {
            //skips turn
            this.Controller.CmdNextTurn();
            return;
        }

        currentCharacter.ResetTurnState();

        //if we don't own that char, swap player turn
        if (this.Controller.PlayerTurn != this.Controller.DraftedCharacterOwners[newTurnCharacterID])
        {
            this.Controller.SwapPlayerTurn();
        }

        this.Controller.AssignControlModesForNewTurn(this.Controller.PlayerTurn, ControlMode.move);
    }
}
