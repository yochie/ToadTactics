using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHUD : MonoBehaviour
{
    #region Editor vars

    [SerializeField]
    private GameEventSOListener characterAddedToTurnOrderListener;

    [SerializeField]
    private GameObject turnOrderSlotPrefab;

    #endregion

    //TODO : make private once character turn change is handled here instead of gamecontroller
    public readonly List<TurnOrderSlotUI> turnOrderSlots = new();

    public static TurnOrderHUD Singleton { get; set; }

    private void Awake()
    {
        TurnOrderHUD.Singleton = this;
    }

    #region Events

    public void OnCharacterAddedToTurnOrder()
    {
        Debug.Log("Adding character to turn order.");
        GameObject slot = Instantiate(this.turnOrderSlotPrefab, this.transform);
        turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());
        this.UpdateTurnOrderSlotsUI();
    }

    //TODO : hookup with event once we have this happening (ie clones)
    public void OnCharacterRemovedFromTurnOrder(int classID)
    {
        foreach (TurnOrderSlotUI currentSlot in turnOrderSlots)
        {
            if (currentSlot.holdsCharacterWithClassID == classID)
            {
                //Debug.LogFormat("Destroying slot with {0}", AllPlayerCharPrefabs[value].name);
                this.turnOrderSlots.Remove(currentSlot);
                Destroy(currentSlot.gameObject);
                break;
            }
        }
        this.UpdateTurnOrderSlotsUI();
    }

    #endregion

    #region Utility
    //fills all slots with appopriate sprite and classID from sortedTurnOrder
    private void UpdateTurnOrderSlotsUI()
    {
        //Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach (float initiative in GameController.Singleton.sortedTurnOrder.Keys)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            if (i >= this.turnOrderSlots.Count) return;

            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            Image slotImage = slot.GetComponent<Image>();
            int classID = GameController.Singleton.sortedTurnOrder[initiative];
            slotImage.sprite = GameController.Singleton.AllPlayerCharPrefabs[classID].GetComponent<SpriteRenderer>().sprite;
            slot.holdsCharacterWithClassID = classID;

            if (GameController.Singleton.turnOrderIndex == i)
            {
                slot.HighlightAndLabel(i + 1);
            }
            else
            {
                slot.UnhighlightAndLabel(i + 1);
            }
            i++;
        }
    }
    #endregion
}
