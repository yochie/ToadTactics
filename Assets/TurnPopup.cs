using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnPopup : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI label;
    
    [SerializeField]
    GameObject popup;

    [SerializeField]
    GameObject background;

    [SerializeField]
    float slideDurationSeconds;

    [SerializeField]
    float stillDurationSeconds;

    [SerializeField]
    Transform endPosition;

    [SerializeField]
    AudioClip wooshSound;

    public void OnLocalPlayerTurnStart()
    {
        IEnumerator popupRoutine = this.PopupCoroutine("Your turn");
        if (AnimationSystem.Singleton != null)
            AnimationSystem.Singleton.Queue(popupRoutine);
        else
            StartCoroutine(popupRoutine);
    }

    public void OnLocalPlayerTurnEnd()
    {
        IEnumerator popupRoutine = this.PopupCoroutine("Opponent turn");
        if (AnimationSystem.Singleton != null)
            AnimationSystem.Singleton.Queue(popupRoutine);
        else
            StartCoroutine(popupRoutine);
    }

    public IEnumerator PopupCoroutine(string labelText)
    {
        this.background.SetActive(true);
        this.popup.SetActive(true);        
        this.label.text = labelText;
        float elapsedSeconds = 0f;
        Vector3 screenCenterPosition = Vector3.zero;
        Vector3 startPosition = this.popup.transform.localPosition;

        AudioManager.Singleton.PlaySoundEffect(this.wooshSound);
        while (elapsedSeconds < slideDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popup.transform.localPosition = Vector3.Lerp(this.popup.transform.localPosition, screenCenterPosition, elapsedSeconds / slideDurationSeconds);
            yield return null;
        }

        yield return new WaitForSeconds(this.stillDurationSeconds);

        AudioManager.Singleton.PlaySoundEffect(this.wooshSound);
        elapsedSeconds = 0f;
        while (elapsedSeconds < slideDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popup.transform.localPosition = Vector3.Lerp(this.popup.transform.localPosition, this.endPosition.localPosition, elapsedSeconds / slideDurationSeconds);
            yield return null;
        }

        this.background.SetActive(false);
        this.popup.transform.localPosition = startPosition;
        this.popup.SetActive(false);
    }
}
