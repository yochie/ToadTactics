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
    private AudioSource musicSource;

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

    }

    public void PlaySoundEffect(AudioClip soundEffect)
    {
        this.effectsSource.PlayOneShot(soundEffect);
    }

    public IEnumerator PlaySoundAndWaitCoroutine(AudioClip soundEffect, float earlyExitSeconds = 0.25f)
    {
        this.PlaySoundEffect(soundEffect);
        yield return new WaitForSeconds(soundEffect.length - earlyExitSeconds);

    }

    internal void SetVolume(float value)
    {
        this.effectsSource.volume = value;
        this.musicSource.volume = value;
    }

    internal float GetEffectsVolume()
    {
        return this.effectsSource.volume;
    }

    internal float GetMusicVolume()
    {
        return this.musicSource.volume;
    }

    public void LoopSong(AudioClip songClip)
    {
        //if trying to play current song, just keep at it
        if (this.musicSource.clip == songClip)
            return;

        this.musicSource.Stop();
        this.musicSource.clip = songClip;
        this.musicSource.Play();
    }
}
