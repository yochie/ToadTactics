using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotsHUD : MonoBehaviour
{

    #region Editor vars
    [SerializeField]
    private GameObject characterSlotPrefab;

    [SerializeField]
    private MapInputHandler inputHandler;
    #endregion

    public static CharacterSlotsHUD Singleton {get; set;}

    //Todo: spawn at runtime to allow gaining new slots for clone or losing slots for amalgam
    private List<CharacterSlotUI> characterSlots = new();


    private void Awake()
    {
        CharacterSlotsHUD.Singleton = this;
    }

    public void OnCharacterPlaced(int classID)
    {
        foreach (CharacterSlotUI slot in this.characterSlots)
        {
            if (slot.HoldsCharacterWithClassID == classID)
            {
                slot.HasBeenPlacedOnBoard = true;
            }
        }
    }

    public void OnCharAddedToTurnOrder(int classID)
    {
        if (!GameController.Singleton.HeOwnsThisCharacter(GameController.Singleton.LocalPlayer.playerID, classID))
            return;

        GameObject characterSlotObject = Instantiate(this.characterSlotPrefab, this.transform);        
        CharacterSlotUI characterSlot = characterSlotObject.GetComponent<CharacterSlotUI>();
        characterSlot.SetInputHandler(this.inputHandler);
        characterSlots.Add(characterSlot);

        characterSlot.GetComponent<Image>().sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        characterSlot.HoldsCharacterWithClassID = classID;
    }
}
