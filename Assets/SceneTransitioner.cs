using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float transitionDurationSeconds;

    void Start()
    {
        this.animator.SetTrigger("FadeIn");
    }

    public void ChangeScene(Action doSceneChange)
    {
        Debug.Log("Transitioning scenes");
        StartCoroutine(FadeOutThenChangeScene(doSceneChange));
    }

    private IEnumerator FadeOutThenChangeScene(Action doSceneChange)
    {
        animator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(transitionDurationSeconds);

        doSceneChange();
    }
}
