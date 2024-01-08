using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro text;

    [SerializeField]
    private SpriteRenderer critIcon;

    internal void Init(int damage, Color popupColor, bool isCrit)
    {
        
        text.color = popupColor;
        this.text.text = (damage >= 0 ? "-" : "+") + Mathf.Abs(damage).ToString();
        if (isCrit)
        {
            this.critIcon.gameObject.SetActive(true);
            this.critIcon.color = popupColor;
        }
    }

    public void SetAlpha(float alpha)
    {
        this.text.alpha = alpha;
        this.critIcon.color = Utility.SetAlpha(critIcon.color, alpha);
    }
}
