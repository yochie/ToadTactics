using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedSceneTransitioner : NetworkBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float transitionDurationSeconds;

    [ClientRpc]
    public void RpcFadeout()
    {
        Debug.Log("Transitioning scenes");

        Action after = () => {
            GameController.Singleton.CmdNotifySceneFadeOutComplete();        
        };

        StartCoroutine(FadeOutThenChangeScene(after));

    }

    private IEnumerator FadeOutThenChangeScene(Action after)
    {
        animator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(transitionDurationSeconds);

        after();
    }
}
