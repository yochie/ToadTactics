using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicCharacterSlotUI : MonoBehaviour
{
    [SerializeField]
    private Image spriteImage;

    public int HoldsCharacterWithClassID { get; set; }

    public void InitBasicsWithClassID(int classID)
    {
        this.HoldsCharacterWithClassID = classID;
        this.spriteImage.sprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);
    }
}
