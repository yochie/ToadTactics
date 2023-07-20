using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDraftPhase : IGamePhase
{
    public GameController Controller { get; set; }

    public string Name { get; set; }

    public GamePhaseID ID => GamePhaseID.characterDraft;

    public void Init(string name, GameController controller)
    {
        this.Name = name;
        this.Controller = controller;

        //Roll 6 classes

        //fill slots


    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }
}
