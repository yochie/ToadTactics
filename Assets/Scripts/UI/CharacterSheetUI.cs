using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class CharacterSheetUI : MonoBehaviour
{
    public int holdsClassID;

    [SerializeField]
    private GameObject content;

    [SerializeField]
    private GameObject background;

    [SerializeField]
    private Image spriteImage;

    [SerializeField]
    private TextMeshProUGUI nameLabel;

    [SerializeField]
    private TextMeshProUGUI descriptionLabel;

    [SerializeField]
    private AbilitiesTable abilitiesTable;

    [SerializeField]
    private StatsTable statsTable;

    [SerializeField]
    private EquipmentTable equipmentTable;

    [SerializeField]
    private GameObject closeButton;

    [SerializeField]
    private GameEventSO characterSheetClosedEvent;

    #region Startup

    public void FillWithClassData(int classID, bool isAKing)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClassDefaults(classData);
        this.statsTable.RenderCharacterStats(classData.stats, isAKing);
    }

    public void FillWithActiveCharacterData(int classID, CharacterStats currentStats, bool isAKing, List<string> equipmentIDs, PlayerCharacter character)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForLiveCharacter(character, withCooldowns: GameController.Singleton.CurrentPhaseID == GamePhaseID.gameplay);
        this.statsTable.RenderCharacterStats(currentStats, isAKing);
        GameObject equipmentRow = this.equipmentTable.transform.parent.gameObject;
        equipmentRow.SetActive(equipmentIDs.Count > 0);
        this.equipmentTable.SetupWithEquipments(equipmentIDs);
    }

    #endregion

    #region Events

    //Called by close button
    public void CloseSheet()
    {
        this.content.SetActive(false);
        this.background.SetActive(false);

        this.characterSheetClosedEvent.Raise();
    }

    public void OnCharacterSheetDisplayed(int classID)
    {
        if (!GameController.Singleton.PlayerCharactersByID.ContainsKey(classID) || GameController.Singleton.PlayerCharactersByID[classID] == null)
            this.FillWithClassData(classID, GameController.Singleton.IsAKing(classID));
        else
        {
            PlayerCharacter activeCharacter = GameController.Singleton.PlayerCharactersByID[classID];
            this.FillWithActiveCharacterData(classID, activeCharacter.CurrentStats, GameController.Singleton.IsAKing(classID), activeCharacter.EquipmentIDsCopy, activeCharacter);
        }
        this.content.SetActive(true);
        this.background.SetActive(true);
    }
    #endregion
}
