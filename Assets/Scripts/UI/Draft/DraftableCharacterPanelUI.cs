using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableCharacterPanelUI : NetworkBehaviour
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
    private Button draftButton;

    [SerializeField]
    private Button crownButton;

    [SerializeField]
    private Image grayOutPanel;

    private bool assigningKing = false;

    #region Startup
    [TargetRpc]
    public void TargetRpcInitForDraft(NetworkConnectionToClient target, int classID)
    {
        this.Init(classID);
    }

    public void Init(int classID, bool asKingCandidate = false)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClassDefaults(classData);
        this.statsTable.RenderForInactiveCharacterStats(classData.stats, asKingCandidate);
        this.assigningKing = asKingCandidate;

        if (asKingCandidate)
        {
            //hides draft buttons
            this.SetButtonActiveState(state: false, asKingCandidate: false);
            //displays crown buttons
            this.SetButtonActiveState(state: true, asKingCandidate: true);
        }
        else
            //hide draft buttons at start, enabled after dice roll
            this.SetButtonActiveState(false);
    }

    [TargetRpc]
    internal void TargetRpcEnableDraftButton(NetworkConnectionToClient target)
    {
        this.SetButtonActiveState(state: true);
    }

    #endregion

    #region Events
    //handler for event
    public void OnCharacterDrafted(int playerID, int classID){
        if (this.holdsClassID != classID || this.assigningKing)
            return;

        this.SetButtonActiveState(false);
        this.grayOutPanel.gameObject.SetActive(true);
    }

    //handler for event
    public void OnCharacterCrowned(int classID)
    {
        this.SetButtonActiveState(state: false, asKingCandidate: true);
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

    #endregion

    internal void SetButtonActiveState(bool state, bool asKingCandidate = false)
    {
        if (asKingCandidate)
            this.crownButton.gameObject.SetActive(state);
        else
            this.draftButton.gameObject.SetActive(state);
    }
}
