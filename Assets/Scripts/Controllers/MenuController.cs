using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        MyNetworkManager.singleton.StartHost();
    }    

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game should be closed");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}