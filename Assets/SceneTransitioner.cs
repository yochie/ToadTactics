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

    //Set here OR whenever NetworkedSceneTransitioner takes care of transition
    public bool FadeOutTriggered { get; set; }

    void Start()
    {
        //skip fade in when entering lobby to avoid abrupt transitions for client connections
        if (SceneManager.GetActiveScene().name == "Lobby")
            return;

        this.animator.SetTrigger("FadeIn");
    }

    public void FadeOut(Action afterFadeOut)
    {
        this.FadeOutTriggered = true;
        Debug.Log("Transitioning scenes");
        StartCoroutine(FadeOutThenChangeScene(afterFadeOut));
    }

    private IEnumerator FadeOutThenChangeScene(Action after)
    {
        this.animator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(transitionDurationSeconds);

        after();
    }
}
