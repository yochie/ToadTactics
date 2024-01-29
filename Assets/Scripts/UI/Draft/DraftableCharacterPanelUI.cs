using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableCharacterPanelUI : MonoBehaviour
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

    [SerializeField]
    private DraftUI draftUI;

    //used to ignore late characterDrafted events once weve already setup king assignment
    private bool assigningKing = false;

    #region Startup

    public void Init(int classID, bool forKingAssignment = false)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderForClassDefaults(classData);
            
        this.assigningKing = forKingAssignment;

        if (forKingAssignment)
        {
            this.statsTable.RenderCharacterStatsForKingAssignment(classData.stats);
            //hides draft buttons
            this.SetButtonActiveState(enabled: false, asKingCandidate: false);
            //displays crown buttons
            this.SetButtonActiveState(enabled: true, asKingCandidate: true);
        }
        else {
            this.statsTable.RenderCharacterStats(classData.stats, isAKing: false);
            //hide draft buttons at start, enabled after dice roll
            this.SetButtonActiveState(false);
        }
    }

    internal void EnableDraftButton()
    {
        this.SetButtonActiveState(enabled: true);
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
        this.SetButtonActiveState(enabled: false, asKingCandidate: true);
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
        //Server will validate to avoid bad/repeated inputs, no need to disable local input ui immediately
        GameController.Singleton.LocalPlayer.CmdDraftCharacter(this.holdsClassID);
    }

    //called by button
    public void CrownCharacter()
    {
        //Server will validate to avoid bad/repeated inputs, no need to disable local input ui immediately
        GameController.Singleton.LocalPlayer.CmdCrownCharacter(this.holdsClassID);
    }

    #endregion

    internal void SetButtonActiveState(bool enabled, bool asKingCandidate = false)
    {
        if (asKingCandidate)
            this.crownButton.gameObject.SetActive(enabled);
        else
            this.draftButton.gameObject.SetActive(enabled);
    }
}
