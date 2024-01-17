using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float transitionDurationSeconds;

    void Start()
    {
        //skip fade in when entering lobby to avoid abrupt transitions for client connections
        if (SceneManager.GetActiveScene().name == "Lobby")
            return;

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
