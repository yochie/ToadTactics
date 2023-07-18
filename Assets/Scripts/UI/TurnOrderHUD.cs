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
    private IntGameEventSOListener turnOrderChangedListener;

    [SerializeField]
    private GameObject turnOrderSlotPrefab;

    #endregion

    //TODO : make private once character turn change is handled here instead of gamecontroller
    public readonly List<TurnOrderSlotUI> turnOrderSlots = new();

    public static TurnOrderHUD Singleton { get; set; }

    private void Awake()
    {
        TurnOrderHUD.Singleton = this;

        this.turnOrderChangedListener.Response.AddListener(OnTurnOrderIndexChanged);
    }

    #region Events

    private void OnTurnOrderIndexChanged(int newTurnIndex)
    {
        //Debug.Log("OnCharacterTurnChanged");

        //finds class ID for character whose turn it is
        int newTurnCharacterId = GameController.Singleton.ClassIdForTurn(newTurnIndex);

        //highlights turnOrderSlotUI for currently playing character
        this.highlightSlot(newTurnCharacterId);
    }

    public void OnCharacterAddedToTurnOrder()
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

    #endregion

    #region Utility
    //fills all slots with appopriate sprite and classID from sortedTurnOrder
    private void ResetTurnOrderSlots()
    {
        //Debug.Log("Updating turnOrderSlots");
        int i = 0;
        foreach (float initiative in GameController.Singleton.sortedTurnOrder.Keys)
        {
            //stops joining clients from trying to fill slots that weren't created yet
            if (i >= this.turnOrderSlots.Count) return;

            int classID = GameController.Singleton.sortedTurnOrder[initiative];
            TurnOrderSlotUI slot = this.turnOrderSlots[i];
            slot.SetSprite(ClassDataSO.Singleton.GetSpriteByClassID(classID));
            slot.holdsCharacterWithClassID = classID;

            if (GameController.Singleton.AllCharactersAreOnBoard())
            {
                PlayerCharacter currentCharacter = GameController.Singleton.playerCharacters[classID];
                slot.SetLifeLabel(currentCharacter.CurrentLife(), currentCharacter.currentStats.maxHealth);
            }
            else
            {
                CharacterClass currentClass = ClassDataSO.Singleton.GetClassByID(classID);
                slot.SetLifeLabel(currentClass.stats.maxHealth, currentClass.stats.maxHealth);
            }

            if (GameController.Singleton.turnOrderIndex == i)
            {
                slot.Highlight();
            }
            else
            {
                slot.Unhighlight();
            }
            i++;
        }
    }

    private void highlightSlot(int classID)
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
