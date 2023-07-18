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

        Controller.turnOrderIndex = 0;
        Controller.playerTurn = 0;

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = -1;
        int i = 0;
        foreach (int classID in Controller.sortedTurnOrder.Values)
        {
            if (i == Controller.turnOrderIndex)
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
        if (Controller.playerTurn != Controller.characterOwners[currentCharacterClassID])
        {
            Controller.SwapPlayerTurn();
        }

        Controller.assignControlModesForNewTurn(Controller.playerTurn, ControlMode.move);
        Controller.RpcOnInitGameplayMode();
    }

    [Server]
    public void Tick()
    {
        //loops through turn order        
        Controller.turnOrderIndex++;
        if (Controller.turnOrderIndex >= Controller.sortedTurnOrder.Count)
            Controller.turnOrderIndex = 0;

        //finds character class id for the next turn so that we can check who owns it
        int currentCharacterClassID = -1;
        int i = 0;
        foreach (int classID in Controller.sortedTurnOrder.Values)
        {
            if (i == Controller.turnOrderIndex)
            {
                currentCharacterClassID = classID;
            }
            i++;
        }
        if (currentCharacterClassID == -1)
        {
            Debug.Log("Error : Bad code for iterating turn order");
        }

        PlayerCharacter currentCharacter = Controller.playerCharacters[currentCharacterClassID];
        currentCharacter.ResetTurnState();

        //if we don't own that char, swap player turn
        if (Controller.playerTurn != Controller.characterOwners[currentCharacterClassID])
        {
            Controller.SwapPlayerTurn();
        }

        Controller.assignControlModesForNewTurn(Controller.playerTurn, ControlMode.move);
    }
}
