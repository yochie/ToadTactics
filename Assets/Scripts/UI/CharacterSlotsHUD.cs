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
    private List<GameplayCharacterSlotUI> characterSlots = new();


    private void Awake()
    {
        CharacterSlotsHUD.Singleton = this;
    }

    public void OnCharacterPlaced(int classID)
    {
        foreach (GameplayCharacterSlotUI slot in this.characterSlots)
        {
            if (slot.HoldsCharacterWithClassID == classID)
            {
                slot.HasBeenPlacedOnBoard = true;
            }
        }
    }

    public void InitSlots(List<int> classIDs)
    {
        foreach(int classID in classIDs)
        {
            if (!GameController.Singleton.HeOwnsThisCharacter(GameController.Singleton.LocalPlayer.playerID, classID))
                return;
        
            GameObject characterSlotObject = Instantiate(this.characterSlotPrefab, this.transform);        
            GameplayCharacterSlotUI characterSlot = characterSlotObject.GetComponent<GameplayCharacterSlotUI>();
            characterSlot.SetInputHandler(this.inputHandler);
        
            characterSlots.Add(characterSlot);

            characterSlot.GetComponent<Image>().sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

            characterSlot.HoldsCharacterWithClassID = classID;

            if (GameController.Singleton.IsAKing(classID))
                characterSlot.DisplayCrown();
        }
    }
}
