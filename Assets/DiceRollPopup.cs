using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollPopup : NetworkBehaviour
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
    AnimationCurve growthCurve;

    [SerializeField]
    Image topBorder;

    [SerializeField]
    Image bottomBorder;

    [TargetRpc]
    public void TargetRpcShowRollOutcome(NetworkConnectionToClient target, bool youStart)
    {

        //Check char ownership instead of player turn to avoid race condition
        Color color = youStart ? Color.green : Color.red;
        string text = youStart ? "You get first pick" : "Opponent gets first pick";
        IEnumerator popupRoutine = this.PopupCoroutine(text, color);
        StartCoroutine(popupRoutine);
    }

    public IEnumerator PopupCoroutine(string text, Color color)
    {

        this.label.text = text;
        this.label.color = color;        
        this.topBorder.color = color;
        this.bottomBorder.color = color;
        this.background.SetActive(true);
        this.popup.SetActive(true);

        AudioManager.Singleton.PlaySoundEffect(this.animationSound);

        float elapsedSeconds = 0f;
        Vector3 startingScale = this.popup.transform.localScale;
        while (elapsedSeconds < this.animationDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popupCanvasGroup.alpha = this.fadeCurve.Evaluate(elapsedSeconds / this.animationDurationSeconds);
            float growth = this.growthCurve.Evaluate(elapsedSeconds / this.animationDurationSeconds);
            this.popup.transform.localScale = startingScale * (1 + growth);
            yield return null;
        }

        this.popup.transform.localScale = startingScale;
        this.background.SetActive(false);
        this.popup.SetActive(false);
        GameController.Singleton.CmdDiceRollPlayed();
    }
}
