using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftedCharacterSlotUI : ActiveCharacterSlotUI
{
    public void OnCharacterCrowned(int classID)
    {
        if (this.HoldsCharacterWithClassID == classID)
            this.CrownImage.gameObject.SetActive(true);
    }
}
