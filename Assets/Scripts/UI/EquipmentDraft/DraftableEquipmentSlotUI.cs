using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableEquipmentSlotUI : MonoBehaviour
{
    public string holdsEquipmentID;

    [SerializeField]
    private Image spriteImage;

    [SerializeField]
    private TextMeshProUGUI nameLabel;

    [SerializeField]
    private TextMeshProUGUI descriptionLabel;

    [SerializeField]
    private StatsTable statsTable;

    [SerializeField]
    private GameObject draftButton;

    [SerializeField]
    private Image grayOutPanel;

    #region Startup

    public void Awake()
    {
        Debug.LogFormat("{0} has awoken", this);
    }

    public void Init(string equipmentID)
    {
        this.holdsEquipmentID = equipmentID;

        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);

        this.spriteImage.sprite = equipmentData.Sprite;
        this.nameLabel.text = equipmentData.NameUI;
        this.descriptionLabel.text = equipmentData.Description;

        if (typeof(IStatModifier).IsAssignableFrom(equipmentData.GetType()))
        {
            IStatModifier statEquipment = equipmentData as IStatModifier;
            this.statsTable.RenderForStatEquipment(statEquipment);
        }

        this.SetButtonActiveState(enabled: false);
    }

    #endregion

    #region Events
    //public void OnEquipmentDrafted(string equipmentID, int playerID)
    //{
    //    if (this.holdsEquipmentID != equipmentID)
    //        return;

    //    this.grayOutPanel.gameObject.SetActive(true);
    //    this.SetButtonActiveState(false);
    //}

    //called by button
    public void DraftEquipment()
    {
        GameController.Singleton.LocalPlayer.CmdDraftEquipment(this.holdsEquipmentID);
    }

    #endregion

    #region Utility
    internal void SetButtonActiveState(bool enabled)
    {
            this.draftButton.SetActive(enabled);
    }

    public void GrayOut()
    {
        this.grayOutPanel.gameObject.SetActive(true);
    }
    #endregion
}