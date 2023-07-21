using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

        Debug.Log(this.Name);
        Debug.Log(this.Controller);
        Debug.Log(GameObject.FindWithTag("DraftUI"));

        this.Controller.draftUI = GameObject.FindWithTag("DraftUI").GetComponent<DraftUI>();
        Debug.Log(this.Controller.draftUI);
        this.Controller.draftUI.Init();

    }

    [Server]
    public void Tick()
    {
        throw new System.NotImplementedException();
    }
}
