using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class AssignmentCharacterSheetUI : MonoBehaviour
{
    public int holdsClassID;

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
    private GameObject assignEquipmentButton;

    #region Startup

    public void Init(int classID, bool asKing = false)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);


        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClass(classData);
        this.statsTable.RenderForBaseStats(classData.stats, asKing);

        this.SetButtonActiveState(state: true);

    }

    #endregion

    #region Events

    //called by button
    public void AssignEquipment()
    {
        GameController.Singleton.LocalPlayer.CmdAssignEquipment(GameController.Singleton.equipmentDraftUI.currentlyAssigningEquipmentID, this.holdsClassID);
    }

    public void OnEquipmentAssigned(string equipmentID, int playerID, int classID)
    {
        if (classID == this.holdsClassID
            && playerID == GameController.Singleton.LocalPlayer.playerID
            && equipmentID == GameController.Singleton.equipmentDraftUI.currentlyAssigningEquipmentID)
        {
            //TODO : add stats and equipment icon
        }
    }

    #endregion

    internal void SetButtonActiveState(bool state)
    {
        this.assignEquipmentButton.SetActive(state);
    }
    
}
