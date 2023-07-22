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
    GameObject crownSpriteImage;


    public int holdsCharacterWithClassID = -1;

    private bool isHighlighted = false;
    private bool IsHighlighted
    {
        set
        {
            isHighlighted = value;
            Color oldColor = this.highlightImage.color;
            this.highlightImage.color = Utility.SetHighlight(oldColor, value);
        }
    }

    internal void Highlight()
    {
        this.IsHighlighted = true;
    }

    internal void Unhighlight()
    {
        this.IsHighlighted = false;
    }

    internal void SetLifeLabel(int currentLife, int maxHealth)
    {
        this.lifeLabel.text = String.Format("{0}/{1}", currentLife, maxHealth);
    }

    internal void SetSprite(Sprite sprite)
    {
        this.characterImage.sprite = sprite;
    }
    internal void ShowCrown()
    {
        this.crownSpriteImage.SetActive(true);
    }
    internal void HideCrown()
    {
        this.crownSpriteImage.SetActive(false);
    }
}
