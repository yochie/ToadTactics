using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForClientPhase : IGamePhase
{
    public GameController Controller {get; set;}

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.waitingForClient;

    public void Init(string name, GameController controller)
    {
        this.Name = name;
        this.Controller = controller;
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }
}
