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
    private IntGameEventSOListener characterPlacedListener;

    [SerializeField]
    private IntIntGameEventSOListener characterDraftedListener;

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

    private void Start()
    {
        this.characterPlacedListener.Response.AddListener(OnCharacterPlaced);
        this.characterDraftedListener.Response.AddListener(OnCharacterDrafted);
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

    public void OnCharacterDrafted(int draftedByPlayerID, int classID)
    {
        if (GameController.Singleton.LocalPlayer.playerID != draftedByPlayerID)
            return;        

        GameObject characterSlotObject = Instantiate(this.characterSlotPrefab, this.transform);        
        CharacterSlotUI characterSlot = characterSlotObject.GetComponent<CharacterSlotUI>();
        characterSlot.SetInputHandler(this.inputHandler);
        characterSlots.Add(characterSlot);

        characterSlot.GetComponent<Image>().sprite = GameController.Singleton.GetCharPrefabWithClassID(classID).GetComponent<SpriteRenderer>().sprite;

        characterSlot.HoldsCharacterWithClassID = classID;
    }
}
