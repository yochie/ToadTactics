using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffPopup : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer render;

    internal void Init(Sprite sprite, Color popupColor)
    {

        this.render.sprite = sprite;
        this.render.color = popupColor;
    }

    public void SetAlpha(float alpha)
    {
        render.color = Utility.SetAlpha(render.color, alpha);
    }
}