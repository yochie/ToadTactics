using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffectSpawner : MonoBehaviour
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
        Instantiate(effectAnimation, this.spawnLocation.position + spawnOffsets, Quaternion.AngleAxis(spawnRotation, Vector3.forward), forCharacter.transform);
    }

}
