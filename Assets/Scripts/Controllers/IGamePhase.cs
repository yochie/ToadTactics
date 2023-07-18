using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePhase
{
    public GameController Controller { get;}

    public string Name { get; set; }

    public GamePhaseID ID { get; }

    public void Tick();

    public void Init(string name, GameController controller);

}
