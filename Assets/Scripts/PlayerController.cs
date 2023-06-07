using Mirror;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : NetworkBehaviour
{
    public static int numPlayers = 0;

    GameController gc;

    public override void OnStartClient()
    {
        base.OnStartClient();

        this.gc = GameController.Singleton;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        foreach (CharacterSlotUI slot in gc.CharacterSlotsUI)
        {
            slot.LocalPlayer = this;
        }

        //for now just choose random chars
        //TODO : fill these using draft eventually
        for (int i = 0; i < this.gc.CharacterSlotsUI.Length; i++)
        {
            int prefabIndex = Random.Range(0, this.gc.AllPlayerCharPrefabs.Length - 1);
            PlayerCharacter newChar = this.gc.AllPlayerCharPrefabs[prefabIndex].GetComponent<PlayerCharacter>();
            CharacterSlotUI slot = this.gc.CharacterSlotsUI[i];

            slot.GetComponent<Image>().sprite = newChar.GetComponent<SpriteRenderer>().sprite;

            slot.HoldsPlayerCharacterWithIndex = prefabIndex;
        }
    }

    [Command]
    public void CmdCreateChar(int playerCharacterIndex, Vector3 position)
    {
        GameObject newChar = 
            Instantiate(gc.AllPlayerCharPrefabs[playerCharacterIndex], new Vector3(0, 0, 0), Quaternion.identity);

        NetworkServer.Spawn(newChar, connectionToClient);

        this.RpcPlaceChar(newChar, position);
    }

    [ClientRpc]
    public void RpcPlaceChar(GameObject character, Vector3 position)
    {
        character.transform.position = position + Map.characterOffsetOnMap;
    }

    public override void OnStartServer() 
    {

    }

    public override void OnStopServer() {

    }

}
