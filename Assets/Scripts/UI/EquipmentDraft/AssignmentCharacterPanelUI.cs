using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class AssignmentCharacterPanelUI : MonoBehaviour
{
    private int holdsClassID;

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
    
    [SerializeField]
    private EquipmentTable equipmentTable;

    #region Startup

    public void Init(int classID, List<string> previouslyAssignedEquipmentIDs, bool asKing = false)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClassDefaults(classData);
        this.statsTable.RenderForInactiveCharacterStats(classData.stats, asKing);
        GameObject equipmentRow = this.equipmentTable.transform.parent.gameObject;
        equipmentRow.SetActive(previouslyAssignedEquipmentIDs.Count > 0);
        this.equipmentTable.SetupWithEquipments(previouslyAssignedEquipmentIDs);
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
            GameObject equipmentRow = this.equipmentTable.transform.parent.gameObject;
            equipmentRow.SetActive(true);
            this.equipmentTable.AddEquipment(equipmentID);
        }
    }

    #endregion

    internal void SetButtonActiveState(bool state)
    {
        this.assignEquipmentButton.SetActive(state);
    }
    
}
