using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteShaker : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve shakeStrengthCurve;

    [SerializeField]
    private float shakeDuration;

    [SerializeField]
    private float shakeStrength;

    public IEnumerator ShakeCoroutine(float shakeDuration = -1, float shakeStrength = -1, AnimationCurve shakeStrengthCurve = null)
    {
        if (shakeDuration == -1)
            shakeDuration = this.shakeDuration;
        if (shakeStrength == -1)
            shakeStrength = this.shakeStrength;
        if (shakeStrengthCurve == null)
            shakeStrengthCurve = this.shakeStrengthCurve;

        Vector3 startPosition = gameObject.transform.position;
        float elapsedSeconds = 0f;

        while (elapsedSeconds < shakeDuration)
        {
            elapsedSeconds += Time.deltaTime;
            float currentStrength = shakeStrengthCurve.Evaluate(elapsedSeconds / shakeDuration) * shakeStrength;
            gameObject.transform.position = startPosition + (UnityEngine.Random.insideUnitSphere * currentStrength);
            yield return null;
        }

        gameObject.transform.position = startPosition;
    }
}
