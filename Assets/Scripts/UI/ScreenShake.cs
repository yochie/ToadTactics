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

    public void ShakeOnCharacterAttack(int classID)
    {
        this.StartCoroutine(ShakeCoroutine(shakeDuration, shakeStrength));
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
