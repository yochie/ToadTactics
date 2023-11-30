using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro text;

    internal void Init(int damage, Color popupColor)
    {
        
        text.color = popupColor;
        this.text.text = (damage > 0 ? "-" : "+") + Mathf.Abs(damage).ToString();

    }

    public void SetAlpha(float alpha)
    {
        text.alpha = alpha;
    }
}
