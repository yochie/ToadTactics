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

    #region Startup

    public void FillWithClassData(int classID, bool isAKing)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClass(classData);
        this.statsTable.RenderForBaseStats(classData.stats, isAKing);
    }

    public void FillWithActiveCharacterData(int classID, CharacterStats currentStats, bool isAKing, List<string> equipmentIDs)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClass(classData);
        this.statsTable.RenderForCurrentStats(currentStats, isAKing);
        GameObject equipmentRow = this.equipmentTable.transform.parent.gameObject;
        equipmentRow.SetActive(equipmentIDs.Count > 0);
        this.equipmentTable.RenderWithEquipments(equipmentIDs);
    }

    #endregion

    #region Events

    //Called by close button
    public void CloseSheet()
    {
        this.content.SetActive(false);
    }

    public void OnCharacterSheetDisplayed(int classID)
    {
        if (!GameController.Singleton.PlayerCharactersByID.ContainsKey(classID) || GameController.Singleton.PlayerCharactersByID[classID] == null)
            this.FillWithClassData(classID, GameController.Singleton.IsAKing(classID));
        else
        {
            PlayerCharacter activeCharacter = GameController.Singleton.PlayerCharactersByID[classID];
            this.FillWithActiveCharacterData(classID, activeCharacter.CurrentStats, GameController.Singleton.IsAKing(classID), activeCharacter.EquipmentIDsCopy);
        }
        this.content.SetActive(true);
    }
    #endregion
}
