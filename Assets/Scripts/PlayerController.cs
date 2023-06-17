using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    //0 for host
    //1 for client
    public int playerIndex;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer && this.isOwned) {
            this.playerIndex = 0;
        } else if (isServer && !this.isOwned)
        {
            this.playerIndex = 1;
        } else if (!isServer && this.isOwned)
        {
            this.playerIndex = 1;
        } else if (!isServer && !this.isOwned)
        {
            this.playerIndex = 0;
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
        List<int> usedPrefabs = new();

        //remove characters used by other clients
        foreach (int characterId in GameController.Singleton.characterOwners.Keys)
        {
            usedPrefabs.Add(characterId);
        }
        for (int i = 0; i < GameController.Singleton.characterSlotsUI.Count; i++)
        {            
            int prefabIndex;
            do
            {
                prefabIndex = Random.Range(0, GameController.Singleton.AllPlayerCharPrefabs.Length);

            }
            while (usedPrefabs.Contains(prefabIndex));
            usedPrefabs.Add(prefabIndex);

            PlayerCharacter newChar = GameController.Singleton.AllPlayerCharPrefabs[prefabIndex].GetComponent<PlayerCharacter>();
            CharacterSlotUI slot = GameController.Singleton.characterSlotsUI[i];

            slot.GetComponent<Image>().sprite = newChar.GetComponent<SpriteRenderer>().sprite;

            slot.HoldsCharacterWithPrefabID = prefabIndex;

            GameController.Singleton.CmdAddCharToTurnOrder(this.playerIndex, GameController.AllClasses[newChar.className].charStats.initiative, prefabIndex);
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();

    }
   
    public override void OnStopServer()
    {
        base.OnStopServer();

        //removes clients player data when he exits as long as server is still running
        if (!NetworkServer.active || GameController.Singleton.LocalPlayer == this) return;
        GameController.Singleton.RemoveAllMyChars(this.playerIndex);
        GameController.Singleton.ResetCharacterTurn();

    }
}
