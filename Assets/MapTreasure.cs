using Mirror;
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

    [ClientRpc]
    public void RpcOpenAnimation(bool val)
    {
        List<IEnumerator> coroutineBatch = new();
        coroutineBatch.Add(this.SetVisibleCoroutine(val));
        coroutineBatch.Add(this.IconPopupCoroutine(this.popupIcon, this.flashDurationSeconds, Color.yellow));
        coroutineBatch.Add(this.PlaySoundEffect(this.soundEffect));
        AnimationSystem.Singleton.Queue(coroutineBatch);
    }

    private IEnumerator SetVisibleCoroutine(bool val)
    {
        this.render.enabled = val;
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

    private IEnumerator PlaySoundEffect(AudioClip soundEffect)
    {
        AudioManager.Singleton.PlaySoundEffect(soundEffect);
        yield return new WaitForSeconds(soundEffect.length - 0.25f);
    }
}
