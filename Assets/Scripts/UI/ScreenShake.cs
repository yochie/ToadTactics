using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve shakeStrengthCurve;

    [SerializeField]
    private float shakeDuration;
    [SerializeField]
    private float shakeStrength;

    public IEnumerator TriggerScreenShake(float overrideStrength = -1, float overrideDuration = -1)
    {
        float duration = overrideDuration > 0 ? overrideDuration : this.shakeDuration;
        float strength = overrideStrength > 0 ? overrideStrength : this.shakeStrength;
        return this.ShakeCoroutine(duration, strength);
    }

    private IEnumerator ShakeCoroutine(float durationSeconds, float strengthMultiplier)
    {
        
        Vector3 startPosition = gameObject.transform.position;
        float elapsedSeconds = 0f;
        
        while(elapsedSeconds < durationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            float currentStrength = shakeStrengthCurve.Evaluate(elapsedSeconds / durationSeconds) * strengthMultiplier;
            gameObject.transform.position = startPosition + (Random.insideUnitSphere * currentStrength);
            yield return null;
        }

        gameObject.transform.position = startPosition;
    }
}
