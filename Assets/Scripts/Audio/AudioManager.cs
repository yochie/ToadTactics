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
    private Coroutine currentMusicCoroutine;

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
        this.currentMusicCoroutine = null;
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

    internal void SetVolume(float value, VolumeType volumeType)
    {
        switch(volumeType)
        {
            case VolumeType.music:
                this.musicSource.volume = value;
                return;
            case VolumeType.SFX:
                this.effectsSource.volume = value;
                return;
        }
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
        //if (this.musicSource.clip == songClip)
        //    return;

        this.musicSource.Stop();
        this.musicSource.clip = songClip;
        this.musicSource.Play();
    }

    public void LoopSongQueue(List<AudioClip> songQueue)
    {
        if(this.currentMusicCoroutine != null)
            StopCoroutine(this.currentMusicCoroutine);
        this.currentMusicCoroutine = StartCoroutine(this.PlaySongQueueCoroutine(songQueue));
    }

    private IEnumerator PlaySongQueueCoroutine(List<AudioClip> songQueue)
    {
        this.musicSource.Stop();
        while (true)
        {
            foreach (AudioClip song in songQueue)
            {
                Debug.LogFormat("Starting next song in queue : {0}", song.name);
                this.PlaySong(song);
                while (this.musicSource.isPlaying)
                {
                    //Debug.LogFormat("Waiting for {0} to end.", song.name);
                    yield return new WaitForSecondsRealtime(1);
                }                    
            }
            yield return new WaitForSecondsRealtime(1);
            Debug.Log("Looping whole song queue.");
            Debug.Log(songQueue);
            Debug.Log(songQueue.Count);

        }
    }

    public void LoopMenuSongs()
    {
        Debug.Log("Starting menu music.");
        if(!this.playingMenuMusic)
            this.LoopSongQueue(SongListSO.Singleton.GetRandomMenuSongQueue());
        this.playingMenuMusic = true;
    }

    public void LoopGameplaySongs()
    {
        Debug.Log("Starting gameplay music.");
        this.playingMenuMusic = false;
        this.LoopSongQueue(SongListSO.Singleton.GetRandomGameplaySongQueue());
    }
}
