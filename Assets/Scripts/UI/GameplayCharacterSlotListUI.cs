using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCharacterSlotListUI : BasicCharacterSlotListUI, IOwnedByPlayer
{ 

    [SerializeField]
    private MapInputHandler inputHandler;
    
    [field: SerializeField]
    public bool IsForSelf { get; set; }

    private void Awake()
    {
        if(this.IsForSelf)
            GameController.Singleton.OwnCharacterSlotList = this;
        else
            GameController.Singleton.OpponentCharacterSlotList = this;

    }

    public void OnCharacterPlaced(int classID)
    {
        foreach (GameplayCharacterSlotUI slot in this.slotList)
        {
            if (slot.HoldsCharacterWithClassID == classID)
            {
                slot.HasBeenPlacedOnBoard = true;
            }
        }
    }

    public new void Init(List<int> classIDs)
    {
        base.Init(classIDs);
        foreach(BasicCharacterSlotUI basicSlot in this.slotList)
        {
            GameplayCharacterSlotUI gameplaySlot = basicSlot as GameplayCharacterSlotUI;
            if (gameplaySlot == null)
                throw new System.Exception("Gameplay character slot list prefab is not a gameplay slot.");
            
            gameplaySlot.IsForSelf = this.IsForSelf;

            gameplaySlot.SetMapInputHandler(this.inputHandler);

            if (GameController.Singleton.IsAKing(gameplaySlot.HoldsCharacterWithClassID))
                gameplaySlot.DisplayCrown(true);

            if (this.IsForSelf)
                gameplaySlot.IsHighlighted = true;

        }
    }
}
