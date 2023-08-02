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

    public void OnCharacterAddedToTurnOrder(int classID)
    {
        //Debug.Log("Adding character to turn order.");
        GameObject slot = Instantiate(this.turnOrderSlotPrefab, this.transform);
        turnOrderSlots.Add(slot.GetComponent<TurnOrderSlotUI>());
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
    //should only be called to init characters lives, otherwise it we be reset
    private void ResetTurnOrderSlots()
    {
        //Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach (int classID in GameController.Singleton.sortedTurnOrder.Values)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            //happens because this function is sometimes called out of order on clients
            //todo change turn order to non synced var and maintain in parrallel
            if (i >= this.turnOrderSlots.Count) return;

            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(classID));
            slot.holdsCharacterWithClassID = classID;

            if (GameController.Singleton.turnOrderIndex == i)
                slot.Highlight();
            else
                slot.Unhighlight();

            if (GameController.Singleton.IsAKing(classID))
                slot.ShowCrown();
            else
                slot.HideCrown();


            CharacterClass currentClass = ClassDataSO.Singleton.GetClassByID(classID);
            if (GameController.Singleton.IsAKing(classID))
            {
                int buffedLife = Utility.ApplyKingLifeBuff(currentClass.stats.maxHealth);
                slot.SetLifeLabel(buffedLife, buffedLife);
            }
            else
                slot.SetLifeLabel(currentClass.stats.maxHealth, currentClass.stats.maxHealth);

            i++;
        }

    }

    //called by Attack event
    //Characters should have been created previously
    public void UpdateLifeLabels()
    {
        //Debug.Log("Resetting life labels");
        foreach (TurnOrderSlotUI slot in this.turnOrderSlots)
        {
            int classID = slot.holdsCharacterWithClassID;

            int currentHealth; 
            int maxHealth;
            if (!GameController.Singleton.playerCharacters.ContainsKey(classID) || GameController.Singleton.playerCharacters[classID] == null)
            {
                currentHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
                maxHealth = ClassDataSO.Singleton.GetClassByID(classID).stats.maxHealth;
            }
            else
            {
                PlayerCharacter currentCharacter = GameController.Singleton.playerCharacters[classID];
                currentHealth = currentCharacter.CurrentLife();
                maxHealth = currentCharacter.currentStats.maxHealth;
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
