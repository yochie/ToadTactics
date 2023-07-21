using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftedCharacterSlotUI : MonoBehaviour
{

    [SerializeField]
    Image spriteImage;

    public void SetSprite(Sprite sprite)
    {
        this.spriteImage.sprite = sprite;   
    }
}
