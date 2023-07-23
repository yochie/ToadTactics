using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class TurnOrderSlotUI : MonoBehaviour
{
    [SerializeField]
    private Image highlightImage;

    [SerializeField]
    private Image characterImage;

    [SerializeField]
    private TextMeshProUGUI lifeLabel;

    [SerializeField]
    private Image crownSpriteImage;

    public int holdsCharacterWithClassID = -1;

    private void SetHighlight(bool state)
    {
        Color oldColor = this.highlightImage.color;
        this.highlightImage.color = Utility.SetHighlight(oldColor, state);
    }

    internal void Highlight()
    {
        this.SetHighlight(true);
    }

    internal void Unhighlight()
    {
        this.SetHighlight(false);
    }

    internal void SetLifeLabel(int currentLife, int maxHealth)
    {
        //Debug.Log(currentLife);
        //Debug.Log(maxHealth);
        this.lifeLabel.text = String.Format("{0}/{1}", currentLife, maxHealth);
    }

    internal void SetSprite(Sprite sprite)
    {
        this.characterImage.sprite = sprite;
    }
    internal void ShowCrown()
    {
        this.crownSpriteImage.gameObject.SetActive(true);
    }
    internal void HideCrown()
    {
        this.crownSpriteImage.gameObject.SetActive(false);
    }

    public void OnCharacterDeath(int classID)
    {
        if (this.holdsCharacterWithClassID == classID)
        {
            this.characterImage.color = Utility.GrayOutColor(this.characterImage.color, true);
            this.crownSpriteImage.color = Utility.GrayOutColor(this.characterImage.color, true);
        }
    }

    public void OnCharacterResurrect(int classID)
    {
        if (this.holdsCharacterWithClassID == classID)
        {
            this.characterImage.color = Utility.GrayOutColor(this.characterImage.color, false);
            this.crownSpriteImage.color = Utility.GrayOutColor(this.characterImage.color, false);
        }
    }
}
