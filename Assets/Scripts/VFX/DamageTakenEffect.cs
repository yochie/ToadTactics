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


    public void OnCharacterTakesHit(Hit hit, int classID)
    {
        if (classID != this.forCharacter.CharClassID)
            return;

        Color flashColor = this.damageTakenColor;
        if (hit.damageType == DamageType.healing)
            flashColor = this.healTakenColor;
        
        AnimationSystem.Singleton.Queue(new List<IEnumerator>() { this.FlashCoroutine(flashColor, this.flashDurationSeconds) });
    }

    IEnumerator FlashCoroutine(Color flashColor, float flashDuration)
    {
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        Color startColor = this.forCharacter.BaseColor;

        float elapsedSeconds = 0f;

        while (elapsedSeconds < flashDuration)
        {
            elapsedSeconds += Time.deltaTime;

            renderer.color = Color.Lerp(startColor, flashColor, flashCurve.Evaluate(elapsedSeconds/flashDuration));
            yield return null;
        }

        renderer.color = startColor;
    }

}
