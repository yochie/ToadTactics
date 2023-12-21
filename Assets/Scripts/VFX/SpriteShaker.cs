using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteShaker : MonoBehaviour
{
    //Set defaults in editor
    [SerializeField]
    private AnimationCurve shakeStrengthCurve;

    [SerializeField]
    private float shakeDuration;

    [SerializeField]
    private float shakeStrength;

    [SerializeField]
    private float growthFactor;


    public IEnumerator ShakeCoroutine(float shakeDuration = -1, float shakeStrength = -1, float growthFactor = -1, AnimationCurve shakeStrengthCurve = null)
    {
        //If any parameter is omitted, fall back on editor defaults
        if (shakeDuration == -1)
            shakeDuration = this.shakeDuration;
        if (shakeStrength == -1)
            shakeStrength = this.shakeStrength;
        if (shakeStrengthCurve == null)
            shakeStrengthCurve = this.shakeStrengthCurve;
        if (growthFactor == -1)
            growthFactor = this.growthFactor;

        Vector3 startPosition = gameObject.transform.position;
        float elapsedSeconds = 0f;
        Vector3 startScale = gameObject.transform.localScale;

        while (elapsedSeconds < shakeDuration)
        {
            elapsedSeconds += Time.deltaTime;
            float curveEval = shakeStrengthCurve.Evaluate(elapsedSeconds / shakeDuration);
            float currentStrength =  curveEval * shakeStrength;
            float currentGrowth = curveEval * growthFactor;
            gameObject.transform.position = startPosition + (UnityEngine.Random.insideUnitSphere * currentStrength);
            gameObject.transform.localScale = startScale * (1 + currentGrowth);
            yield return null;
        }

        gameObject.transform.position = startPosition;
        gameObject.transform.localScale = startScale;
    }
}
