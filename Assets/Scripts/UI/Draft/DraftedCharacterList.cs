using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftedCharacterList : BasicCharacterSlotListUI, IOwnedByPlayer
{
    [field: SerializeField]
    public bool IsForSelf { get; set; }

    public void OnCharacterDrafted(int draftedByPlayerID, int draftedClassID)
    {
        bool addToThisList = false;
        if (this.IsForSelf && draftedByPlayerID == GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;
        else if (!this.IsForSelf && draftedByPlayerID != GameController.Singleton.LocalPlayer.playerID)
            addToThisList = true;

        if (!addToThisList)
            return;

        this.AddBasicSlotToList(draftedClassID);
    }
}
