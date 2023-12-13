using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{

    [SerializeField]
    private PlayerCharacter forCharacter;

    [SerializeField]
    private AudioClip deathSoundEffect;
    
    [SerializeField]
    private AudioClip resurrectionSoundEffect;

    [SerializeField]
    private SpriteShaker shaker;

    [SerializeField]
    private float fadeAnimationDuration;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public void OnCharacterDeath(int classID)
    {
        if (classID == this.forCharacter.CharClassID)
        {
            Color fadeToColor = Utility.GrayOutColor(this.forCharacter.BaseColor, true);
            List<IEnumerator> deathEffects = new();
            deathEffects.Add(this.FadeAnimationCoroutine(fadeToColor, this.fadeAnimationDuration, this.spriteRenderer));
            deathEffects.Add(AudioManager.Singleton.PlaySoundAndWaitCoroutine(this.deathSoundEffect));
            deathEffects.Add(this.shaker.ShakeCoroutine());
            AnimationSystem.Singleton.Queue(deathEffects);
        }
    }

    public void OnCharacterResurrect(int classID)
    {
        if (classID == this.forCharacter.CharClassID)
        {
            Color fadeToColor = Utility.GrayOutColor(this.forCharacter.BaseColor, false);
            List<IEnumerator> resurrectionEffects = new();
            resurrectionEffects.Add(this.FadeAnimationCoroutine(fadeToColor, this.fadeAnimationDuration, this.spriteRenderer));
            resurrectionEffects.Add(AudioManager.Singleton.PlaySoundAndWaitCoroutine(this.resurrectionSoundEffect));
            resurrectionEffects.Add(this.shaker.ShakeCoroutine());

            AnimationSystem.Singleton.Queue(resurrectionEffects);
        }
    }

    private IEnumerator FadeAnimationCoroutine(Color fadeToColor, float animationDuration, SpriteRenderer spriteRenderer)
    {
        Color startColor = spriteRenderer.color;

        float elapsedSeconds = 0f;

        while (elapsedSeconds < animationDuration)
        {
            elapsedSeconds += Time.deltaTime;

            spriteRenderer.color = Color.Lerp(startColor, fadeToColor, elapsedSeconds / animationDuration);
            yield return null;
        }
    }

}
