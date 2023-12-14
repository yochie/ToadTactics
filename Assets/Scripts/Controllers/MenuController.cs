using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class MenuController : MonoBehaviour
{
    public static MenuController Singleton {get; private set;}

    private string connectionTarget = "localhost";

    [SerializeField]
    private ConnectionFeedback connectionFeedback;
    
    [SerializeField]
    private GameObject connectionPanel;

    [SerializeField]
    private float connectionAttemptTimeout;

    [SerializeField]
    private Button connectionButton;

    public void Awake()
    {
        if (MenuController.Singleton != null)
            Destroy(MenuController.Singleton.gameObject);
        MenuController.Singleton = this;
    }

    public void SetConnectionTarget(string target)
    {
        this.connectionTarget = target;
    }

    public void StartHost()
    {
        MyNetworkManager.singleton.StartHost();
    }

    public void JoinGame()
    {
        this.connectionPanel.SetActive(true);
    }

    public void ConnectToHost()
    {        
        if (this.connectionTarget != "localhost" && !Regex.IsMatch(this.connectionTarget, Utility.IPV4Regex))
        {
            this.connectionFeedback.Display("Invalid IP");
            return;
        }
        else
        {
            this.connectionFeedback.Hide();
        }

        Debug.Log(Uri.CheckHostName(this.connectionTarget));
        NetworkManager.singleton.networkAddress = this.connectionTarget;

        MyNetworkManager.singleton.StartClient();
        StartCoroutine(this.CheckForConnectionFailureCoroutine(this.connectionAttemptTimeout));
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game should be closed");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private IEnumerator CheckForConnectionFailureCoroutine(float timeOutSeconds)
    {
        float elapsedSeconds = 0f;

        this.connectionButton.interactable = false;

        while (elapsedSeconds < timeOutSeconds && !NetworkClient.isConnected && NetworkClient.active)
        {
            elapsedSeconds += Time.deltaTime;

            yield return null;
        }

        if (!NetworkClient.isConnected)
        {
            this.connectionFeedback.Display("Connection failed");
            NetworkManager.singleton.StopClient();
        }
        else
        {
            this.connectionFeedback.Hide();
        }
        this.connectionButton.interactable = true;
    }
}
