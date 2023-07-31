using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableEquipmentSlotUI : NetworkBehaviour
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

    #region Startup

    public void Awake()
    {
        Debug.LogFormat("{0} has awoken", this);
    }

    [TargetRpc]
    public void TargetRpcInitForDraft(NetworkConnectionToClient target, string equipmentID, bool itsYourTurn)
    {
        this.Init(equipmentID, itsYourTurn);
    }

    public void Init(string equipmentID, bool itsYourTurn)
    {
        //register to Equipment Draft UI on each client
        GameController.Singleton.equipmentDraftUI.RegisterSpawnedSlot(this);
        this.transform.SetParent(GameController.Singleton.equipmentDraftUI.DraftEquipmentSheetsList.transform, false);
        this.holdsEquipmentID = equipmentID;

        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);

        this.spriteImage.sprite = equipmentData.Sprite;
        this.nameLabel.text = equipmentData.NameUI;
        this.descriptionLabel.text = equipmentData.Description;

        if (typeof(IStatModifier).IsAssignableFrom(equipmentData.GetType()))
        {
            IStatModifier statEquipment = equipmentData as IStatModifier;
            this.statsTable.RenderForEquipment(statEquipment);
        }

        //shows draft buttons if its your turn
        this.SetButtonActiveState(state: itsYourTurn);
    }

    #endregion

    #region Events
    //redundant with Update in draft UI, but might avoid issues when that update takes too much time by triggering earlier
    public void OnEquipmentDrafted(string equipmentID, int playerID)
    {
        if (this.holdsEquipmentID != equipmentID)
            return;

        this.SetButtonActiveState(false);
    }

    //called by button
    public void DraftEquipment()
    {
        GameController.Singleton.LocalPlayer.CmdDraftEquipment(this.holdsEquipmentID);
    }

    #endregion

    #region Utility
    internal void SetButtonActiveState(bool state)
    {
            this.draftButton.SetActive(state);
    }
    #endregion
}