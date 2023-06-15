using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class TurnOrderSlotUI : MonoBehaviour
{
    private Image highlightImage;

    public int holdsPrefabWithIndex = -1;

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
    public bool IsHighlighted
    {
        get { return isHighlighted;  }
        set
        {
            isHighlighted = value;
            Color oldColor = this.highlightImage.color;
            Color invisibleColor = oldColor;
            Color transparentColor = oldColor;
            transparentColor.a = 0.5f;
            invisibleColor.a = 0f;
            this.highlightImage.color = value ? transparentColor : invisibleColor;
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

    internal void HighlightAndLabel(int i)
    {
        this.InitiativeLabel = i.ToString() + "*";
        this.IsHighlighted = true;
    }

    internal void UnhighlightAndLabel(int i)
    {
        this.InitiativeLabel = i.ToString();
        this.IsHighlighted = false;
    }
}
