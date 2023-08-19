using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;
using System;

public class ActiveCharacterSlotUI : BasicCharacterSlotUI, IDisplaysCharacterSheet, ICrownable
{

    [field: SerializeField]
    public Image CrownImage { get; set; }

    [field: SerializeField]
    public IntGameEventSO OnCharacterSheetDisplayedEvent { get; set; }

    public void DisplayCrown(bool state)
    {
        this.CrownImage.gameObject.SetActive(state);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        this.OnCharacterSheetDisplayedEvent.Raise(this.HoldsCharacterWithClassID);
    }
}
