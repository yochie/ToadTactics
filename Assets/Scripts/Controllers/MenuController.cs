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
    private GameObject optionsPanel;

    [SerializeField]
    private float connectionAttemptTimeout;

    [SerializeField]
    private Button connectionButton;

    [SerializeField]
    private SceneTransitioner sceneTransitioner;

    public void Awake()
    {
        if (MenuController.Singleton != null)
            Destroy(MenuController.Singleton.gameObject);
        MenuController.Singleton = this;

        //Cleanup previous session DDOL objects
        if (GameController.Singleton != null)
        {
            Destroy(GameController.Singleton.gameObject);
            Debug.Log("Previous GameController destroyed");
        }

        if (MasterLogger.Singleton != null) { 
            Destroy(MasterLogger.Singleton.gameObject);
            Debug.Log("Previous MasterLogger destroyed");
        }

        if (DebugLogger.Singleton != null)
        {
            Destroy(DebugLogger.Singleton.gameObject);
            Debug.Log("Previous DebugLogger destroyed");
        }
    }

    public void Start()
    {
        AudioManager.Singleton.LoopMenuSongs();
    }

    #region Connection
    public void SetConnectionTarget(string target)
    {
        this.connectionTarget = target;
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
    #endregion

    #region Button responses
    public void StartHost()
    {
        this.SetMenuMode(MenuMode.None);
        MyNetworkManager.singleton.StartHost();
        //this.sceneTransitioner.ChangeScene(() => MyNetworkManager.singleton.StartHost());
    }

    public void OpenSettingsPanel()
    {
        this.SetMenuMode(MenuMode.Settings);
    }

    public void OpenConnectionPanel()
    {
        this.SetMenuMode(MenuMode.Connecting);
    }

    public void ExitGame()
    {
        this.SetMenuMode(MenuMode.None);

        Application.Quit();
        Debug.Log("Game should be closed");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    #endregion

    #region menu state
    public void SetMenuMode(MenuMode menuMode)
    {
        this.HideAllPanels();
        switch (menuMode)
        {
            case MenuMode.Connecting:
                this.connectionPanel.SetActive(true);
                break;
            case MenuMode.Settings:
                this.optionsPanel.SetActive(true);
                break;            
        }        
    }

    public void HideAllPanels()
    {
        this.optionsPanel.SetActive(false);
        this.connectionPanel.SetActive(false);
    }

    public enum MenuMode
    {
        None, Connecting, Settings
    }
    #endregion
}


