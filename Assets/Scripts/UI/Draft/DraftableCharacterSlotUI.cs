using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableCharacterSlotUI : NetworkBehaviour
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
    private GameObject draftButton;

    [SerializeField]
    private GameObject crownButton;

    [SerializeField]
    private GameObject assignEquipmentButton;

    #region Startup
    [TargetRpc]
    public void TargetRpcInitForDraft(NetworkConnectionToClient target, int classID, bool itsYourTurn)
    {
        this.Init(classID, itsYourTurn);
    }

    public void Init(int classID, bool itsYourTurn, bool asKingCandidate = false, bool asEquipmentAssignCandidate = false)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderFromDictionary(classData.GetPrintableAbilityDictionary());
        this.statsTable.RenderFromDictionary(classData.stats.GetPrintableDictionary(), asKingCandidate);


        if (asKingCandidate)
        {
            //hides draft buttons
            this.SetButtonActiveState(state: false, asKingCandidate: false);
            //displays crown buttons
            this.SetButtonActiveState(state: true, asKingCandidate: true);
        }
        else if (asEquipmentAssignCandidate)
        {
            this.SetButtonActiveState(state: true, asKingCandidate: false, asEquipmentAssignCandidate: true);
        }
        else
            this.SetButtonActiveState(itsYourTurn, false);
    }

    #endregion

    #region Events
    //handler for event
    public void OnCharacterDrafted(int playerID, int classID){
        if (this.holdsClassID != classID)
            return;

        this.SetButtonActiveState(false);
    }

    //handler for event
    public void OnCharacterCrowned(int classID)
    {
        this.SetButtonActiveState(false, true);
    }

    public void OnLocalPlayerTurnStart()
    {
        if (GameController.Singleton.CharacterHasBeenDrafted(this.holdsClassID))
            return;

        this.SetButtonActiveState(true);
    }

    public void OnLocalPlayerTurnEnd()
    {
        this.SetButtonActiveState(false);
    }

    //called by button
    public void DraftCharacter()
    {
        GameController.Singleton.LocalPlayer.CmdDraftCharacter(this.holdsClassID);
    }

    //called by button

    public void CrownCharacter()
    {
        GameController.Singleton.LocalPlayer.CmdCrownCharacter(this.holdsClassID);
    }

    //called by button
    public void assignEquipment()
    {
        GameController.Singleton.LocalPlayer.CmdAssignEquipment(GameController.Singleton.equipmentDraftUI.currentlyAssigningEquipmentID, this.holdsClassID);
    }
    #endregion

    internal void SetButtonActiveState(bool state, bool asKingCandidate = false, bool asEquipmentAssignCandidate = false)
    {
        if (asKingCandidate)
            this.crownButton.SetActive(state);
        else if(asEquipmentAssignCandidate)
            this.assignEquipmentButton.SetActive(state);
        else
            this.draftButton.SetActive(state);
    }



}
