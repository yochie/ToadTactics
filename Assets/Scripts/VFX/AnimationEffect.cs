using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffect : MonoBehaviour
{
    private bool isRunning;    
    [SerializeField]
    private Animator animator;

    public void DestroyMe()
    {
        this.isRunning = false;
        //Destroy(gameObject);
    }

    public IEnumerator RunAnimation()
    {
        this.isRunning = true;
        this.animator.SetBool("Started", true);
        while (this.isRunning)
        {
            yield return null;
        }
    }

}
