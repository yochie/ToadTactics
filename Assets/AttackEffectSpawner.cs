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

    public void OnCharacterAttacks(int classID)
    {
        if (classID != forCharacter.CharClassID)
            return;
        Instantiate(effectAnimation, this.spawnLocation.position, Quaternion.AngleAxis(180, Vector3.forward));
    }

}
