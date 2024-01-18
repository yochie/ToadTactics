using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedSceneTransitioner : NetworkBehaviour
{

    [SerializeField]
    private SceneTransitioner localTransitioner;

    //checked on server only to see if current scene has already triggered fading out and avoid double trigger
    public bool HasTriggered { get; set; }

    [ClientRpc]
    public void RpcFadeout()
    {
        //Debug.Log("Transitioning scenes");
        this.HasTriggered = true;

        Action after = () => {
            GameController.Singleton.CmdNotifySceneFadeOutComplete();        
        };

        this.localTransitioner.FadeOut(after);
    }
}
