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

    [SyncVar]
    public bool hasBeenDrafted;

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

    [ClientRpc]
    public void RpcInit(int classID)
    {
        this.Init(classID);
    }

    public void Init(int classID, bool asKingCandidate = false)
    {
        this.holdsClassID = classID;
        this.hasBeenDrafted = false;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderFromDictionary(classData.GetPrintableAbilityDictionary());
        this.statsTable.RenderFromDictionary(classData.stats.GetPrintableDictionary(), asKingCandidate);

        if (asKingCandidate)
            this.SetButtonActiveState(true, true);
        else
            this.SetButtonActiveState(true, false);
    }

    internal void SetButtonActiveState(bool state, bool asKingCandidate = false)
    {
        if (!asKingCandidate)
            this.draftButton.SetActive(state);
        else
            this.crownButton.SetActive(state);
    }

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

    //called by button
    public void DraftCharacter()
    {
        this.CmdSetAsDrafted();
        GameController.Singleton.LocalPlayer.CmdDraftCharacter(this.holdsClassID);
    }

    //called by button

    public void CrownCharacter()
    {
        GameController.Singleton.LocalPlayer.CmdCrownCharacter(this.holdsClassID);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetAsDrafted()
    {
        this.hasBeenDrafted = true;
    }
}
