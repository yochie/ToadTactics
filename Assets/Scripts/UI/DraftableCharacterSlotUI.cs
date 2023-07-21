using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class DraftableSlotUI : NetworkBehaviour
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

    [ClientRpc]
    public void RpcInit(int classID)
    {
        this.holdsClassID = classID;

        //Debug.Log("Rendering draftable");
        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderFromDictionary(classData.GetPrintableAbilityDictionary());
        this.statsTable.RenderFromDictionary(classData.stats.GetPrintableDictionary());

        this.draftButton.GetComponent<Button>().onClick.AddListener(delegate{ GameController.Singleton.draftUI.DraftCharacter(classID); });
    }

    [ClientRpc]
    internal void RpcSetButtonActiveState(bool state)
    {
        this.draftButton.SetActive(state);
    }

    public void OnCharacterDrafted(int playerID, int classID){
        if (this.holdsClassID != classID)
            return;

        this.RpcSetButtonActiveState(false);
    }
}
