using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton { get; private set; }

    [SerializeField]
    private AudioSource effectsSource;

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

    public void PlaySoundEffect(AudioClip clip)
    {
        this.effectsSource.PlayOneShot(clip);
    }
}
