using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaReloadedEffect : MonoBehaviour
{

    [SerializeField]
    private PlayerCharacter forCharacter;

    [SerializeField]
    private AudioClip soundEffect;

    public void OnCharacterReloadedBallista(int classID)
    {
        if (classID == this.forCharacter.CharClassID)
        {
            List<IEnumerator> effects = new();
            effects.Add(AudioManager.Singleton.PlaySoundAndWaitCoroutine(this.soundEffect));
            AnimationSystem.Singleton.Queue(effects);
        }
    }
}
