using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    public void InitSlots(List<TurnOrderSlotInitData> slotDataList, List<int> sortedTurnOrder)
    {
        Debug.Log("Initializing turn order slots.");

        foreach (int orderClass in sortedTurnOrder)
        {
            TurnOrderSlotInitData slotData = slotDataList.First(slotData => slotData.classID == orderClass);
            GameObject slotObject = Instantiate(this.turnOrderSlotPrefab, this.transform);            
            TurnOrderSlotUI slot = slotObject.GetComponent<TurnOrderSlotUI>();
            turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());

            int classID = slotData.classID;
            slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(classID));
            slot.HoldsCharacterWithClassID = classID;

            slot.SetHighlight(slotData.itsHisTurn);
            slot.DisplayCrown(slotData.isAKing);

            slot.SetLifeDisplay(slotData.maxHealth, slotData.maxHealth);

            int i = 0;
            foreach(int buffID in slotData.orderedBuffIDs)
            {
                slot.AddBuffIcon(buffID, slotData.orderedBuffDataIDs[i], slotData.orderedBuffDurations[i]);
                i++;
            }
        }
    }

    //TODO : hookup with event once we have this happening (ie clones?)
    public void OnCharacterRemovedFromTurnOrder(int classID)
    {
        foreach (TurnOrderSlotUI currentSlot in turnOrderSlots)
        {
            if (currentSlot.HoldsCharacterWithClassID == classID)
            {
                //Debug.LogFormat("Destroying slot with {0}", AllPlayerCharPrefabs[value].name);
                this.turnOrderSlots.Remove(currentSlot);
                Destroy(currentSlot.gameObject);
                break;
            }
        }
    }

    public void OnCharacterLifeChanged(int classID, int currentLife, int maxHealth)
    {
        this.UpdateLifeLabels(classID, currentLife, maxHealth);
    }

    #endregion

    #region Utility

    internal void AddBuffIcons(int buffID, List<int> affectedCharacterIDs, string buffDataID, int remainingDuration)
    {

        foreach(TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (affectedCharacterIDs.Contains(slot.HoldsCharacterWithClassID))
            {
                slot.AddBuffIcon(buffID, buffDataID, remainingDuration);
            }
        }
    }

    internal void UpdateBuffIconDurations(int buffID, List<int> affectedCharacterIDs, string buffDataID, int remainingDuration)
    {

        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (affectedCharacterIDs.Contains(slot.HoldsCharacterWithClassID))
            {
                slot.UpdateBuffIconDuration(buffID, buffDataID, remainingDuration);
            }
        }
    }

    internal void RemoveBuffIconFromCharacters(int buffID, List<int> removeFromCharacters)
    {
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (removeFromCharacters.Contains(slot.HoldsCharacterWithClassID))
            {
                slot.RemoveBuffIcon(buffID);
            }
        }
    }

    public void UpdateLifeLabels(int classToUpdate, int currentLife, int maxHealth)
    {
        //Debug.Log("Resetting life labels");
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            int classID = slot.HoldsCharacterWithClassID;
            if (classToUpdate != classID)
                continue;

            //int currentHealth; 
            //int maxHealth;
            //if (!GameController.Singleton.PlayerCharactersByID.ContainsKey(classID) || GameController.Singleton.PlayerCharactersByID[classID] == null)
            //{
            //    currentHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
            //    maxHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
            //}
            //else
            //{
            //    PlayerCharacter currentCharacter = GameController.Singleton.PlayerCharactersByID[classID];
            //    currentHealth = currentCharacter.CurrentLife;
            //    maxHealth = currentCharacter.CurrentStats.maxHealth;
            //}
            slot.SetLifeDisplay(currentLife, maxHealth);
        }
    }

    private void HighlightSlot(int classID)
    {        
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {     
            slot.SetHighlight(slot.HoldsCharacterWithClassID == classID);
        }
    }

    public void DisplayDamagePreview(int classID, int damage, int currentLife, int maxLife)
    {
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            if (classID == slot.HoldsCharacterWithClassID)
            {
                slot.DisplayDamagePreview(damage, currentLife, maxLife);
            }
        }
    }

    public void HideAllDamagePreviews()
    {
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            slot.HideDamagePreview();
        }
    }
    #endregion
}
