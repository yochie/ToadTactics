using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffAccruedEffect : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve flashCurve;

    [SerializeField]
    private float flashDurationSeconds;

    [SerializeField]
    private PlayerCharacter forCharacter;

    [SerializeField]
    private Color positiveColor;

    [SerializeField]
    private Color negativeColor;

    [SerializeField]
    private IconPopup iconPopupPrefab;

    [SerializeField]
    private Vector3 iconPopupOffset;

    [SerializeField]
    private AnimationCurve shakeStrengthCurve;

    [SerializeField]
    private float shakeDuration;

    [SerializeField]
    private float shakeStrength;

    [SerializeField]
    private float shakeGrowthFactor;

    [SerializeField]
    private SpriteShaker spriteShaker;

    [SerializeField]
    private AudioClip positiveSoundEffect;

    [SerializeField]
    private AudioClip negativeSoundEffect;

    public void TriggerBuffEffect(bool isPositive, string buffDataID)
    {
        List<IEnumerator> effectsBatch = new List<IEnumerator>();

        Color flashColor = isPositive ? this.positiveColor : this.negativeColor;
        effectsBatch.Add(this.FlashCoroutine(flashColor, this.flashDurationSeconds));

        Sprite icon = BuffDataSO.Singleton.GetBuffIcon(buffDataID);
        if(icon != null)
            effectsBatch.Add(this.BuffPopupCoroutine(icon, this.flashDurationSeconds, flashColor));

        effectsBatch.Add(this.spriteShaker.ShakeCoroutine(this.shakeDuration, this.shakeStrength, this.shakeGrowthFactor, this.shakeStrengthCurve));

        AudioClip soundEffect = isPositive ? this.positiveSoundEffect : this.negativeSoundEffect;
        effectsBatch.Add(this.PlaySoundCoroutine(soundEffect));

        AnimationSystem.Singleton.Queue(effectsBatch);
    }

    private IEnumerator BuffPopupCoroutine(Sprite icon, float popupDurationSeconds, Color color)
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

    IEnumerator FlashCoroutine(Color flashColor, float flashDurationSeconds)
    {
        SpriteRenderer renderer = this.forCharacter.GetSpriteRenderer();
        Color startColor = this.forCharacter.BaseColor;

        float elapsedSeconds = 0f;

        while (elapsedSeconds < flashDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;

            renderer.color = Color.Lerp(startColor, flashColor, this.flashCurve.Evaluate(elapsedSeconds / flashDurationSeconds));
            yield return null;
        }

        renderer.color = startColor;
    }

    private IEnumerator PlaySoundCoroutine(AudioClip soundEffect)
    {
        AudioManager.Singleton.PlaySoundEffect(soundEffect);
        yield return new WaitForSeconds(soundEffect.length - 0.25f);
    }
}
