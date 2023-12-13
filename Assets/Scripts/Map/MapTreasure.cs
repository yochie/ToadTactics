using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTreasure : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer render;

    [SerializeField]
    private IconPopup iconPopupPrefab;

    [SerializeField]
    private Vector3 iconPopupOffset;

    [SerializeField]
    private Sprite popupIcon;

    [SerializeField]
    private AnimationCurve flashCurve;

    [SerializeField]
    private float flashDurationSeconds;

    [SerializeField]
    private AudioClip soundEffect;

    [SerializeField]
    private float treasureRevealDurationSeconds;

    [ClientRpc]
    public void RpcOpenAnimation(string equipmentID)
    {
        List<IEnumerator> coroutineBatch = new();
        coroutineBatch.Add(this.HideSpriteCoroutine());
        coroutineBatch.Add(this.IconPopupCoroutine(this.popupIcon, this.flashDurationSeconds, Color.yellow));
        coroutineBatch.Add(AudioManager.Singleton.PlaySoundAndWaitCoroutine(this.soundEffect));
        AnimationSystem.Singleton.Queue(coroutineBatch);

        AnimationSystem.Singleton.Queue(this.TreasureRevealPanelCoroutine(equipmentID));
    }

    private IEnumerator TreasureRevealPanelCoroutine(string equipmentID)
    {
        MainHUD.Singleton.DisplayTreasureRevealPanel(equipmentID, display: true);
        yield return new WaitForSeconds(this.treasureRevealDurationSeconds);
        MainHUD.Singleton.DisplayTreasureRevealPanel(equipmentID, display: false);
    }

    private IEnumerator HideSpriteCoroutine()
    {
        this.render.enabled = false;
        yield break;
    }


    private IEnumerator IconPopupCoroutine(Sprite icon, float popupDurationSeconds, Color color)
    {
        IconPopup popup = Instantiate(this.iconPopupPrefab, gameObject.transform.position + this.iconPopupOffset, Quaternion.identity);

        popup.Init(icon, color);

        float elapsedSeconds = 0f;

        while (elapsedSeconds < popupDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;

            popup.SetAlpha(Mathf.Lerp(0, 1, this.flashCurve.Evaluate(elapsedSeconds / popupDurationSeconds)));
            yield return null;
        }

        Destroy(popup.gameObject);
    }

}
