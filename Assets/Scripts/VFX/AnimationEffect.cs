using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffect : MonoBehaviour
{
    private bool isRunning;    

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioClip soundEffect;

    [SerializeField]
    private float soundEarlyExitSeconds;

    public void DestroyMe()
    {
        this.isRunning = false;
        Destroy(gameObject);
    }

    public IEnumerator RunAnimation()
    {
        this.isRunning = true;
        this.animator.SetBool("Started", true);
        if(this.soundEffect != null)
        {
            AudioManager.Singleton.PlaySoundEffect(this.soundEffect);
            //wait for sound to almost finish, some overlap is fine to speed things up
            yield return new WaitForSeconds(this.soundEffect.length - this.soundEarlyExitSeconds);
        }

        //wait for animation to also finish if not already the case
        while (this.isRunning)
        {
            yield return null;
        }
    }
}
