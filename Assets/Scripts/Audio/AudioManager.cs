using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton { get; private set; }

    [SerializeField]
    private AudioSource effectsSource;

    [SerializeField]
    private float volume;

    private void Awake()
    {
        if(AudioManager.Singleton != null)
        {
            Destroy(this.gameObject);
        } else
        {
            AudioManager.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        this.volume = 0.5f;
    }

    public void PlaySoundEffect(AudioClip soundEffect)
    {
        this.effectsSource.PlayOneShot(soundEffect, this.volume);
    }

    public IEnumerator PlaySoundAndWaitCoroutine(AudioClip soundEffect, float earlyExitSeconds = 0.25f)
    {
        this.PlaySoundEffect(soundEffect);
        yield return new WaitForSeconds(soundEffect.length - earlyExitSeconds);

    }

    internal void SetVolume(float value)
    {
        this.volume = value;
    }
}
