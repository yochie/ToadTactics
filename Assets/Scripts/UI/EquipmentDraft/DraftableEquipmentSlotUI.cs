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

    [SerializeField]
    private GameObject assignButton;

    #region Startup

    public void Awake()
    {
        Debug.LogFormat("{0} has awoken", this);
    }

    public void Init(string equipmentID, bool itsYourTurn, bool asAssignmentCandidate = false)
    {
        this.transform.SetParent(GameController.Singleton.equipmentDraftUI.EquipmentSheetsList.transform, false);
        this.holdsEquipmentID = equipmentID;

        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);

        this.spriteImage.sprite = equipmentData.Sprite;
        this.nameLabel.text = equipmentData.NameUI;
        this.descriptionLabel.text = equipmentData.Description;

        if (typeof(IStatModifier).IsAssignableFrom(equipmentData.GetType()))
        {
            IStatModifier statEquipment = equipmentData as IStatModifier;
            this.statsTable.RenderFromDictionary(statEquipment.GetPrintableStatDictionary(), false);
        }

        if (asAssignmentCandidate)
        {
            //hides draft buttons
            this.SetButtonActiveState(state: false, asAssignmentCandidate: false);
            //displays assignment buttons
            this.SetButtonActiveState(state: true, asAssignmentCandidate: true);
        }
        else
            //shows draft buttons if its your turn
            this.SetButtonActiveState(state: itsYourTurn, asAssignmentCandidate: false);
    }

    #endregion

    [TargetRpc]
    public void TargetRpcInitForDraft(NetworkConnectionToClient target, string equipmentID, bool itsYourTurn)
    {
        this.Init(equipmentID, itsYourTurn);
    }

    #region Events
    public void OnEquipmentDrafted(string equipmentID, int playerID)
    {
        if (this.holdsEquipmentID != equipmentID)
            return;

        this.SetButtonActiveState(false);
    }

    public void OnEquipmentAssigned(string equipmentID, int playerID, int classID)
    {
        this.SetButtonActiveState(false, true);
    }

    public void OnLocalPlayerTurnStart()
    {
        EquipmentDraftPhase equipmentDraftPhase = GameController.Singleton.currentPhaseObject as EquipmentDraftPhase;

        if (GameController.Singleton.EquipmentHasBeenDrafted(this.holdsEquipmentID))
            return;

        this.SetButtonActiveState(true);
    }

    public void OnLocalPlayerTurnEnd()
    {
        this.SetButtonActiveState(false);
    }

    //called by button
    //public void DraftEquipment()
    //{
    //    GameController.Singleton.LocalPlayer.CmdDraftEquipment(this.holdsEquipmentID);
    //}

    //called by button

    //public void assignEquipment()
    //{
    //    GameController.Singleton.LocalPlayer.CmdAssignEquipment(this.holdsEquipmentID);
    //}

    #endregion

    internal void SetButtonActiveState(bool state, bool asAssignmentCandidate = false)
    {
        if (!asAssignmentCandidate)
            this.draftButton.SetActive(state);
        else
            this.assignButton.SetActive(state);
    }
}