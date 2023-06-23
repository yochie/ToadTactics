using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    //0 for host
    //1 for client
    public int playerID;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer && this.isOwned) {
            this.playerID = 0;
        } else if (isServer && !this.isOwned)
        {
            this.playerID = 1;
        } else if (!isServer && this.isOwned)
        {
            this.playerID = 1;
        } else if (!isServer && !this.isOwned)
        {
            this.playerID = 0;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GameController.Singleton.LocalPlayer = this;

        //for now just choose random chars
        //TODO : fill these using draft eventually
        List<int> usedClasses = new();

        //make characters used by other clients unavailable
        foreach (int classID in GameController.Singleton.characterOwners.Keys)
        {
            usedClasses.Add(classID);
        }
        for (int i = 0; i < GameController.Singleton.characterSlots.Count; i++)
        {            
            int classID;
            List<int> classIDs = new();
            GameController.Singleton.AllClasses.Keys.CopyTo<int>(classIDs);
            do
            {
                classID = classIDs[Random.Range(0, classIDs.Count)];
            }
            while (usedClasses.Contains(classID));
            usedClasses.Add(classID);

            CharacterClass newCharClass = GameController.Singleton.AllClasses[classID];
            CharacterSlotUI characterSlot = GameController.Singleton.characterSlots[i];

            characterSlot.GetComponent<Image>().sprite = GameController.Singleton.GetCharPrefabWithClassID(classID).GetComponent<SpriteRenderer>().sprite;

            characterSlot.HoldsCharacterWithClassID = classID;

            GameController.Singleton.CmdAddCharToTurnOrder(this.playerID, newCharClass.charStats.initiative, classID);
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();

    }
   
    public override void OnStopServer()
    {
        base.OnStopServer();
    }
}
