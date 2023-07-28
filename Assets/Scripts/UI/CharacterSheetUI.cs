using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class CharacterSheetUI : MonoBehaviour
{
    public int holdsClassID;

    [SerializeField]
    private GameObject content;

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
    private GameObject closeButton;

    #region Startup

    public void FillWithClassData(int classID)
    {
        this.holdsClassID = classID;

        CharacterClass classData = ClassDataSO.Singleton.GetClassByID(classID);
        Sprite sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);

        this.spriteImage.sprite = sprite;
        this.nameLabel.text = classData.name;
        this.descriptionLabel.text = classData.description;
        this.abilitiesTable.RenderFromDictionary(classData.GetPrintableAbilityDictionary());
        this.statsTable.RenderFromDictionary(classData.stats.GetPrintableDictionary(), false);
    }

    #endregion

    #region Events

    //Called by close button
    public void CloseSheet()
    {
        this.content.SetActive(false);
    }

    public void OnCharacterSheetDisplayed(int classID)
    {
        this.FillWithClassData(classID);
        this.content.SetActive(true);
    }
    #endregion
}
