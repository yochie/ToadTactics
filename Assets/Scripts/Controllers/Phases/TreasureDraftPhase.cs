using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.treasureDraft;

    public void Init(string name, GameController controller)
    {
        //skip for now
        GameController.Singleton.CmdChangeToScene("Maingame");
        //MyNetworkManager.singleton.ServerChangeScene("Maingame");
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }
}
