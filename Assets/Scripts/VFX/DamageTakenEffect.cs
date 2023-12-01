using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTakenEffect : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve flashCurve;

    [SerializeField]
    private float flashDurationSeconds;

    [SerializeField]
    private PlayerCharacter forCharacter;
    
    [SerializeField]
    private Color damageTakenColor;

    [SerializeField]
    private Color healTakenColor;

    [SerializeField]
    private Color physDamagePopupColor;

    [SerializeField]
    private Color magicDamagePopupColor;

    [SerializeField]
    private Color healingDamagePopupColor;

    [SerializeField]
    private DamagePopup damagePopupPrefab;

    [SerializeField]
    private Vector3 damagePopupOffset;

    [SerializeField]
    private AnimationCurve shakeStrengthCurve;

    [SerializeField]
    private float shakeDuration;

    [SerializeField]
    private float shakeStrength;

    [SerializeField]
    private AudioClip appleSoundEffect;

    public void OnCharacterTakesHit(Hit hit, int classID)
    {
        if (classID != this.forCharacter.CharClassID)
            return;

        Color flashColor = this.damageTakenColor;
        if (hit.damageType == DamageType.healing)
        {
            flashColor = this.healTakenColor;
            if (hit.hitSource == HitSource.Apple)
            {
                AnimationSystem.Singleton.Queue(new List<IEnumerator>() {
                    this.FlashCoroutine(flashColor, this.flashDurationSeconds),
                    this.DamagePopupCoroutine(hit, this.flashDurationSeconds),
                    this.PlayCrunchSoundCoroutine()
                });
            } else
            {
                //TODO: add healing sound effect
                AnimationSystem.Singleton.Queue(new List<IEnumerator>() {
                this.FlashCoroutine(flashColor, this.flashDurationSeconds),
                this.DamagePopupCoroutine(hit, this.flashDurationSeconds),
            });
            }

        } else
        {
            AnimationSystem.Singleton.Queue(new List<IEnumerator>() {
                this.FlashCoroutine(flashColor, this.flashDurationSeconds),
                this.DamagePopupCoroutine(hit, this.flashDurationSeconds),
                this.ShakeCoroutine(this.shakeDuration, this.shakeStrength)
            });
        }
    }

    private IEnumerator DamagePopupCoroutine(Hit hit, float popupDurationSeconds )
    {
        Color popupColor;
        switch (hit.damageType)
        {
            case DamageType.physical:
                popupColor = this.physDamagePopupColor;
                break;
            case DamageType.magic:
                popupColor = this.magicDamagePopupColor;
                break;
            case DamageType.healing:
                popupColor = this.healingDamagePopupColor;
                break;
            default:
                popupColor = this.physDamagePopupColor;
                break;
        }

        DamagePopup popup = Instantiate(this.damagePopupPrefab, gameObject.transform.position + this.damagePopupOffset, Quaternion.identity);

        popup.Init((hit.damageType == DamageType.healing ? -hit.damage : hit.damage), popupColor);

        float elapsedSeconds = 0f;

        while (elapsedSeconds < popupDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;

            popup.SetAlpha(Mathf.Lerp(0, 1, this.flashCurve.Evaluate(elapsedSeconds / popupDurationSeconds)));
            yield return null;
        }

        Destroy(popup);
    }

    IEnumerator FlashCoroutine(Color flashColor, float flashDurationSeconds)
    {
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        Color startColor = this.forCharacter.BaseColor;

        float elapsedSeconds = 0f;

        while (elapsedSeconds < flashDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;

            renderer.color = Color.Lerp(startColor, flashColor, this.flashCurve.Evaluate(elapsedSeconds/flashDurationSeconds));
            yield return null;
        }

        renderer.color = startColor;
    }

    private IEnumerator ShakeCoroutine(float durationSeconds, float strengthMultiplier)
    {

        Vector3 startPosition = gameObject.transform.position;
        float elapsedSeconds = 0f;

        while (elapsedSeconds < durationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            float currentStrength = shakeStrengthCurve.Evaluate(elapsedSeconds / durationSeconds) * strengthMultiplier;
            gameObject.transform.position = startPosition + (UnityEngine.Random.insideUnitSphere * currentStrength);
            yield return null;
        }

        gameObject.transform.position = startPosition;
    }

    private IEnumerator PlayCrunchSoundCoroutine()
    {
        AudioManager.Singleton.PlaySoundEffect(this.appleSoundEffect);
        yield return new WaitForSeconds(this.appleSoundEffect.length - 0.25f);
    }
}
