using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TurnOrderSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject ownerHighlights;

    [SerializeField]
    private GameObject opponentHighlights;

    [SerializeField]
    private GameObject buffList;

    [SerializeField]
    private GameObject equipmentsList;

    [SerializeField]
    private Image characterImage;

    [SerializeField]
    private TextMeshProUGUI lifeLabel;

    [SerializeField]
    private Image crownSpriteImage;

    [SerializeField]
    private IntGameEventSO onCharacterSheetDisplayed;

    [SerializeField]
    private GameObject blankImagePrefab;

    [SerializeField]
    private Image lifebar;

    [SerializeField]
    private HealPreview healPreview;

    [SerializeField]
    private DamagePreview damagePreview;

    [SerializeField]
    private float highlightScaling;

    private int holdsCharacterWithClassID = -1;
    public int HoldsCharacterWithClassID { get => this.holdsCharacterWithClassID; set { this.holdsCharacterWithClassID = value; } }

    //unique buffID => icon object
    private Dictionary<int, GameObject> displayedBuffs;

    public void SetHighlight(bool highlighted, bool isOwnedByLocalPlayer)
    {
        if (!highlighted)
        {
            this.ownerHighlights.SetActive(false);
            this.opponentHighlights.SetActive(false);
        } 
        else if (highlighted && isOwnedByLocalPlayer)
            this.ownerHighlights.SetActive(true);
        else if (highlighted && !isOwnedByLocalPlayer)
            this.opponentHighlights.SetActive(true);


        if (highlighted)
        {
            this.transform.localScale = new Vector3(this.highlightScaling, this.highlightScaling, 1f);
        }
        else
            this.transform.localScale = Vector3.one;

        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)this.transform.parent);
    }

    internal void SetLifeDisplay(int currentLife, int maxHealth)
    {
        //Debug.Log(currentLife);
        //Debug.Log(maxHealth);
        this.lifebar.transform.localScale = new Vector3((float)currentLife/(float)maxHealth,1f,1f);
        this.lifeLabel.text = String.Format("{0}/{1}", currentLife, maxHealth);
    }

    internal void SetSprite(Sprite sprite)
    {
        this.characterImage.sprite = sprite;
    }

    public void OnCharacterDeath(int classID)
    {
        if (this.HoldsCharacterWithClassID == classID)
        {
            this.characterImage.color = Utility.GrayOutColor(this.characterImage.color, true);
            this.crownSpriteImage.color = Utility.GrayOutColor(this.characterImage.color, true);
        }
    }

    internal void DisplayCrown(bool state)
    {
        this.crownSpriteImage.gameObject.SetActive(state);
    }

    public void OnCharacterResurrect(int classID)
    {
        if (this.HoldsCharacterWithClassID == classID)
        {
            this.characterImage.color = Utility.GrayOutColor(this.characterImage.color, false);
            this.crownSpriteImage.color = Utility.GrayOutColor(this.characterImage.color, false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.onCharacterSheetDisplayed.Raise(this.HoldsCharacterWithClassID);
    }

    public void AddBuffIcon(int buffUniqueID, string buffDataID, int remainingDuration)
    {
        this.buffList.SetActive(true);        
        GameObject buffIcon = Instantiate(this.blankImagePrefab, this.buffList.transform);
        IBuffDataSO buffData = BuffDataSO.Singleton.GetBuffData(buffDataID);
        buffIcon.GetComponent<Image>().sprite = buffData.Icon;
        TooltipContent tooltip = buffIcon.GetComponentInChildren<TooltipContent>(includeInactive: true);        
        tooltip.SetTitle(buffData.UIName);
        tooltip.SetDescription(buffData.GetTooltipDescription());
        Dictionary<string, string> statsDict = buffData.GetBuffStatsDictionary();
        if (remainingDuration != -1)
            statsDict.Add("Remaining", string.Format("{0} turns",remainingDuration));
        tooltip.FillWithDictionary(statsDict);
        if (this.displayedBuffs == null)
            this.displayedBuffs = new();
        this.displayedBuffs.Add(buffUniqueID, buffIcon);
    }

    public void AddEquipmentIcon(string equipmentID)
    {
        this.equipmentsList.SetActive(true);
        GameObject equipmentIcon = Instantiate(this.blankImagePrefab, this.equipmentsList.transform);
        EquipmentSO equipmentData = EquipmentDataSO.Singleton.GetEquipmentByID(equipmentID);
        equipmentIcon.GetComponent<Image>().sprite = equipmentData.Sprite;
        TooltipContent tooltip = equipmentIcon.GetComponentInChildren<TooltipContent>(includeInactive: true);
        tooltip.SetTitle(equipmentData.NameUI);
        IStatModifier equipmentStats = equipmentData as IStatModifier;
        Dictionary<string, string> statsDict = new();
        if (equipmentStats == null)
        {
            Debug.Log("Equipment icon couldn't be displayed : doesn't implement stat modifier interface");
        } else
        {
            statsDict = equipmentStats.GetStatModificationsDictionnary();
        }
        tooltip.FillWithDictionary(statsDict);
    }

    public void UpdateBuffIconDuration(int buffUniqueID, string buffDataID, int remainingDuration)
    {
        if (remainingDuration == -1)
            return;

        if (!this.displayedBuffs.ContainsKey(buffUniqueID))
            return;

        GameObject buffIcon = this.displayedBuffs[buffUniqueID];
        //for sake of prefab simplicity, just regenerate whole tooltip from data instead of only changing duration...
        IBuffDataSO buffData = BuffDataSO.Singleton.GetBuffData(buffDataID);
        TooltipContent tooltip = buffIcon.GetComponentInChildren<TooltipContent>(includeInactive: true);
        Dictionary<string, string> statsDict = buffData.GetBuffStatsDictionary();
        if (remainingDuration != -1)
            statsDict.Add("Remaining", string.Format("{0} turns", remainingDuration));
        tooltip.FillWithDictionary(statsDict);
    }

    public void RemoveBuffIcon(int buffID)
    {
        if (!this.displayedBuffs.ContainsKey(buffID) || this.displayedBuffs[buffID] == null)
            throw new Exception("Error :Slot asked to remove buff Icon it does not display.");

        GameObject icon = this.displayedBuffs[buffID];
        this.displayedBuffs.Remove(buffID);
        Destroy(icon);

        if (this.displayedBuffs.Count == 0)
            this.buffList.SetActive(false);
    }

    internal void DisplayDamagePreview(int damage, int currentLife, int maxLife)
    {
        if (damage > 0)
        {
            float damageAsPortionOfRemainingLife = (float)damage / (float) currentLife;
            damageAsPortionOfRemainingLife = Math.Clamp(damageAsPortionOfRemainingLife, 0, 1);
            this.damagePreview.Display(true, damageAsPortionOfRemainingLife);
            //Debug.Log(damage);
            //Debug.Log(currentLife);
            //Debug.Log(maxLife);
            //Debug.Log(damageAsPortionOfRemainingLife);
        }
        else
        {
            float remainingPortionOfMaxLifeAfterHeal = (float)(currentLife + Math.Abs(damage)) / (float)maxLife;
            remainingPortionOfMaxLifeAfterHeal = Math.Clamp(remainingPortionOfMaxLifeAfterHeal, 0, 1);
            this.healPreview.Display(true, remainingPortionOfMaxLifeAfterHeal);
        }
    }

    internal void HideDamagePreview()
    {
        this.damagePreview.Display(false, 0);
        this.healPreview.Display(false, 0);
    }
}
