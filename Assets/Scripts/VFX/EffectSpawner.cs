using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject effectAnimation;

    [SerializeField]
    private Transform spawnLocation;

    [SerializeField]
    private PlayerCharacter forCharacter;

    [SerializeField]
    private bool forBallista;

    [SerializeField]
    private Vector3 spawnOffsets;

    [SerializeField]
    private float spawnRotation;

    [SerializeField]
    private bool shakeScreen;

    [SerializeField]
    private bool shakeScreenAfterEffect;

    [SerializeField]
    private float shakeScreenStrengthOverride;

    [SerializeField]
    private float shakeScreenDurationOverride;

    public void OnCharacterAttacks(int classID)
    {
        if (classID != forCharacter.CharClassID)
            return;
        this.QueueEffect();
    }

    public void OnCharacterUsesAbility(string abilityID, int classID)
    {
        if (classID != forCharacter.CharClassID)
            return;
        this.QueueEffect();
    }

    public void OnCharacterUsedBallista(int classID)
    {
        if (classID != forCharacter.CharClassID && this.forBallista)
            return;
        this.QueueEffect();
    }

    private void QueueEffect()
    {
        GameObject effectObject = Instantiate(effectAnimation, this.spawnLocation.position + spawnOffsets, Quaternion.AngleAxis(spawnRotation, Vector3.forward), forCharacter.transform);
        AnimationEffect animationEffect = effectObject.GetComponent<AnimationEffect>();
        List<IEnumerator> effectBatch = new List<IEnumerator> { animationEffect.RunAnimation() };

        if (this.shakeScreen && !this.shakeScreenAfterEffect)
        {
            effectBatch.Add(Camera.main.GetComponent<ScreenShake>().TriggerScreenShake(this.shakeScreenStrengthOverride, this.shakeScreenDurationOverride));
        }
        AnimationSystem.Singleton.Queue(effectBatch);

        if (this.shakeScreen && this.shakeScreenAfterEffect)
        {
            AnimationSystem.Singleton.Queue(Camera.main.GetComponent<ScreenShake>().TriggerScreenShake(this.shakeScreenStrengthOverride, this.shakeScreenDurationOverride));
        }
    }

}
