using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDraftCharacterListUI : BasicCharacterSlotListUI
{

    public override void Init(List<int> classIDs)
    {
        base.Init(classIDs);

        foreach (ActiveCharacterSlotUI activeSlot in this.slotList)
        {
            if (GameController.Singleton.IsAKing(activeSlot.HoldsCharacterWithClassID))
            {
                activeSlot.DisplayCrown(true);
            }        
        }
    }
}
