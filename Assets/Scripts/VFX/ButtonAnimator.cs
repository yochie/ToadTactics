using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private AudioClip buttonHoverSound;

    [SerializeField]
    protected AudioClip buttonClickSound;

    [SerializeField]
    private Button forButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!this.forButton.IsInteractable())
            return;
        AudioManager.Singleton.PlaySoundEffect(this.buttonClickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!this.forButton.IsInteractable())
            return;
        AudioManager.Singleton.PlaySoundEffect(this.buttonHoverSound);
    }

}
