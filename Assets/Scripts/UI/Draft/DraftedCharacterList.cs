using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftedCharacterList : BasicCharacterSlotListUI
{
    [SerializeField]
    bool isForSelf;

    //Called by unity event configured in inspector
    public void OnCharacterDrafted(int draftedByPlayerID, int draftedClassID)
    {
        bool addToThisList = false;
        
        if (this.isForSelf && draftedByPlayerID == GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;
        else if (!this.isForSelf && draftedByPlayerID != GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;

        if (!addToThisList)
            return;

        this.AddBasicSlotToList(draftedClassID);
        //GameObject slotObject = Instantiate(this.slotPrefab, this.transform);
        //DraftedCharacterSlotUI slot = slotObject.GetComponent<DraftedCharacterSlotUI>();
        //slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(draftedClassID));
        //slot.SetClassID(draftedClassID);
    }
}
