using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton { get; private set; }

    [SerializeField]
    private AudioSource effectsSource;

    [SerializeField]
    private AudioSource musicSource;
    private bool playingMenuMusic;

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
        this.playingMenuMusic = false;
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

    public void PlaySong(AudioClip songClip)
    {
        this.musicSource.loop = false;
        if (this.musicSource.clip == songClip)
            return;

        this.musicSource.Stop();
        this.musicSource.clip = songClip;
        this.musicSource.Play();
    }

    public void LoopSongQueue(List<AudioClip> songQueue)
    {
        StopCoroutine(nameof(PlaySongQueueCoroutine));
        StartCoroutine(this.PlaySongQueueCoroutine(songQueue));
    }

    private IEnumerator PlaySongQueueCoroutine(List<AudioClip> songQueue)
    {
        this.musicSource.Stop();
        while (true)
        {
            foreach (AudioClip song in songQueue)
            {
                this.PlaySong(song);
                while (this.musicSource.isPlaying)
                    yield return new WaitForSeconds(1);
            }
        }
    }

    public void LoopMenuSongs()
    {
        if(!this.playingMenuMusic)
            this.LoopSongQueue(SongListSO.Singleton.GetRandomMenuSongQueue());
        this.playingMenuMusic = true;
    }

    public void LoopGameplaySongs()
    {
        this.playingMenuMusic = false;
        this.LoopSongQueue(SongListSO.Singleton.GetRandomGameplaySongQueue());
    }
}
