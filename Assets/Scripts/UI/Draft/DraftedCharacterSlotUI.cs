using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftedCharacterSlotUI : MonoBehaviour
{

    [SerializeField]
    Image spriteImage;

    [SerializeField]
    GameObject crownSpriteImage;

    public int heldCharacterClassID;

    public void SetSprite(Sprite sprite)
    {
        this.spriteImage.sprite = sprite;   
    }

    public void SetClassID(int classID)
    {
        this.heldCharacterClassID = classID;
    }

    public void OnCharacterCrowned(int classID)
    {
        if(this.heldCharacterClassID == classID)
            this.crownSpriteImage.SetActive(true);
    }
}
