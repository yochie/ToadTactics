using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHUD : MonoBehaviour
{
    #region Editor vars

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

    public void OnTurnOrderIndexChanged(int newTurnIndex)
    {
        //Debug.Log("OnCharacterTurnChanged");

        //finds class ID for character whose turn it is
        int newTurnCharacterId = GameController.Singleton.GetCharacterIDForTurn(newTurnIndex);

        //highlights turnOrderSlotUI for currently playing character
        this.HighlightSlot(newTurnCharacterId);
    }

    public void InitSlots(List<TurnOrderSlotInitData> slotDataList)
    {
        Debug.Log("Adding character to turn order.");
        foreach(TurnOrderSlotInitData slotData in slotDataList)
        {
            GameObject slot = Instantiate(this.turnOrderSlotPrefab, this.transform);
            turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());
        }
        this.ResetTurnOrderSlots();
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
        this.ResetTurnOrderSlots();
    }

    public void OnCharacterLifeChanged(int classID, int currentLife, int maxHealth)
    {
        this.UpdateLifeLabels();
    }

    #endregion

    #region Utility
    //fills all slots with appopriate sprite and classID from sortedTurnOrder
    //should only be called to init characters lives, otherwise it we be reset to max
    private void ResetTurnOrderSlots()
    {
        //Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach (int classID in GameController.Singleton.SortedTurnOrder.Values)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            //happens because this function is sometimes called out of order by RPCs
            if (i >= this.turnOrderSlots.Count) return;

            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(classID));
            slot.holdsCharacterWithClassID = classID;

            if (GameController.Singleton.TurnOrderIndex == i)
                slot.Highlight();
            else
                slot.Unhighlight();

            if (GameController.Singleton.IsAKing(classID))
                slot.ShowCrown();
            else
                slot.HideCrown();
            
            CharacterStats currentStats = GameController.Singleton.PlayerCharactersByID[classID].CurrentStats;
            slot.SetLifeLabel(currentStats.maxHealth, currentStats.maxHealth);

            i++;
        }

    }    
    internal void AddBuffIcons(int buffID, List<int> affectedCharacterIDs, Sprite sprite)
    {

        foreach(TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (affectedCharacterIDs.Contains(slot.holdsCharacterWithClassID))
            {
                slot.AddBuffIcon(buffID, sprite);
            }
        }
    }

    internal void RemoveBuffIcons(int buffID, List<int> removeFromCharacters)
    {
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (removeFromCharacters.Contains(slot.holdsCharacterWithClassID))
            {
                slot.RemoveBuffIcon(buffID);
            }
        }
    }

    public void UpdateLifeLabels()
    {
        //Debug.Log("Resetting life labels");
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            int classID = slot.holdsCharacterWithClassID;

            int currentHealth; 
            int maxHealth;
            if (!GameController.Singleton.PlayerCharactersByID.ContainsKey(classID) || GameController.Singleton.PlayerCharactersByID[classID] == null)
            {
                currentHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
                maxHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
            }
            else
            {
                PlayerCharacter currentCharacter = GameController.Singleton.PlayerCharactersByID[classID];
                currentHealth = currentCharacter.CurrentLife;
                maxHealth = currentCharacter.CurrentStats.maxHealth;
            }
            slot.SetLifeLabel(currentHealth, maxHealth);
        }
    }

    private void HighlightSlot(int classID)
    {
        int i = 0;
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            i++;
            if (slot.holdsCharacterWithClassID == classID)
            {
                //Debug.Log("Highlighting slot");
                slot.Highlight();
            }
            else
            {
                slot.Unhighlight();

            }
        }
    }
    #endregion
}
