using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTurnPopup : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI label;

    [SerializeField]
    Image image;

    [SerializeField]
    GameObject popup;

    [SerializeField]
    CanvasGroup popupCanvasGroup;

    [SerializeField]
    GameObject background;

    [SerializeField]
    float animationDurationSeconds;

    [SerializeField]
    AudioClip animationSound;

    [SerializeField]
    AnimationCurve fadeCurve;

    [SerializeField]
    Image topBorder;

    [SerializeField]
    Image bottomBorder;

    public static CharacterTurnPopup Singleton { get; private set; }

    private void Awake()
    {
        if (CharacterTurnPopup.Singleton != null)
            Destroy(this.gameObject);
        else
            CharacterTurnPopup.Singleton = this;
    }

    public void TriggerAnimation(int classID, bool yourCharacter)
    {
        string charName = ClassDataSO.Singleton.GetClassByID(classID).name;
        Sprite charSprite = ClassDataSO.Singleton.GetSpriteByClassID(classID);
        //Check char ownership instead of player turn to avoid race condition
        Color textColor = yourCharacter ? Color.green : Color.red;
        IEnumerator popupRoutine = this.PopupCoroutine(charName, charSprite, textColor);
        if (AnimationSystem.Singleton != null)
            AnimationSystem.Singleton.Queue(popupRoutine);
    }

    public IEnumerator PopupCoroutine(string charName, Sprite charSprite, Color textColor)
    {

        this.label.text = string.Format("{0} turn", charName);
        this.label.color = textColor;
        this.image.sprite = charSprite;
        this.topBorder.color = textColor;
        this.bottomBorder.color = textColor;
        this.background.SetActive(true);
        this.popup.SetActive(true);

        AudioManager.Singleton.PlaySoundEffect(this.animationSound);

        float elapsedSeconds = 0f;
        while (elapsedSeconds < this.animationDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popupCanvasGroup.alpha = this.fadeCurve.Evaluate(elapsedSeconds / this.animationDurationSeconds);
            yield return null;
        }

        this.background.SetActive(false);
        this.popup.SetActive(false);
    }
}
