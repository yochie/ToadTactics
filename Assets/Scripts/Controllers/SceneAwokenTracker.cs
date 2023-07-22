using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class SceneAwokenTracker : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameController.Singleton == null)
        {
            Debug.Log("{0} could not find gamecontroller to notify of scene awoken.");
            return;
        }

        GameController.Singleton.NotifySceneAwoken(this.isServer, SceneManager.GetActiveScene().name);
    }
}
