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
    private Vector3 spawnOffsets;

    [SerializeField]
    private float spawnRotation;

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

    private void QueueEffect()
    {
        GameObject effectObject = Instantiate(effectAnimation, this.spawnLocation.position + spawnOffsets, Quaternion.AngleAxis(spawnRotation, Vector3.forward), forCharacter.transform);
        AnimationEffect effect = effectObject.GetComponent<AnimationEffect>();
        AnimationSystem.Singleton.Queue(effect.RunAnimation());
    }

}
