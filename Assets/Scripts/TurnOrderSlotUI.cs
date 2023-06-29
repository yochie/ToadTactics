using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class TurnOrderSlotUI : MonoBehaviour
{
    private Image highlightImage;

    public int holdsCharacterWithClassID = -1;

    private string initiativeLabel;
    public string InitiativeLabel
    {
        get { return this.initiativeLabel; }
        set {
            this.initiativeLabel = value;
            this.GetComponentInChildren<TextMeshProUGUI>().text = value;  
        }
    }
    private bool isHighlighted = false;
    private bool IsHighlighted
    {
        get { return isHighlighted;  }
        set
        {
            isHighlighted = value;
            Color oldColor = this.highlightImage.color;
            this.highlightImage.color = Utility.SetHighlight(oldColor, value);
        }
    }

    public void Awake()
    {
        foreach (Image child in this.GetComponentsInChildren<Image>())
        {
            if (child.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
            {
                this.highlightImage = child;
            }
        }
    }

    internal void HighlightAndLabel(int initiative)
    {
        this.InitiativeLabel = initiative.ToString() + "*";
        this.IsHighlighted = true;
    }

    internal void UnhighlightAndLabel(int initiative)
    {
        this.InitiativeLabel = initiative.ToString();
        this.IsHighlighted = false;
    }
}
