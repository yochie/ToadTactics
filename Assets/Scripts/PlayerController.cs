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

            slot.HoldsPlayerCharacterWithIndex = prefabIndex;

            CmdAddCharToTurnOrder(GameController.Singleton.AllClasses[newChar.className].CharStats.initiative, prefabIndex);
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();

    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (this == GameController.Singleton.LocalPlayer) return;
        GameController.Singleton.RemoveAllMyChars(this.playerIndex);
        GameController.Singleton.ResetCharacterTurn();
    }

    [Command]
    private void CmdAddCharToTurnOrder(int initiative, int prefabIndex)
    {
        if (Utility.ContainsValue(GameController.Singleton.turnOrderSortedPrefabIds, prefabIndex))
        {
            //Todo add support for this
            Debug.Log("Character is already in turnOrder, use CmdUpdateTurnOrder instead.");
            return;
        }
        GameController.Singleton.AddMyChar(this.playerIndex, prefabIndex, initiative);
    }

    [Command]
    public void CmdCreateCharOnBoard(int characterPrefabIndex, Hex destinationHex)
    {
        //Debug.Log(destinationHex.startZoneForPlayerIndex);
        //Debug.Log(this.playerIndex);
        
        //validate destination
        if (destinationHex == null ||
            !destinationHex.isStartingZone ||
            destinationHex.startZoneForPlayerIndex != this.playerIndex ||
            destinationHex.holdsCharacter != null)
        {
            Debug.Log("Invalid character destination");
            return;
        }

        GameObject characterPrefab = GameController.Singleton.AllPlayerCharPrefabs[characterPrefabIndex];
        string prefabClassName = characterPrefab.GetComponent<PlayerCharacter>().className;
        Vector3 destinationWorldPos = destinationHex.transform.position;
        GameObject newChar = 
            Instantiate(characterPrefab, destinationWorldPos, Quaternion.identity);
        //TODO : Create other classes and set their name in prefabs
        newChar.GetComponent<PlayerCharacter>().Initialize(GameController.Singleton.AllClasses[prefabClassName]);
        NetworkServer.Spawn(newChar, connectionToClient);

        //update Hex state, synced to clients by syncvar
        destinationHex.holdsCharacter = newChar.GetComponent<PlayerCharacter>();

        //this.gc.map.PlacePlayerChar();


        Map.Singleton.RpcPlaceChar(newChar, destinationWorldPos);
    }
}
