using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EscapeMenuController : MonoBehaviour
{
    private bool isOpened;

    [SerializeField]
    private GameObject grayOutPanel;

    [SerializeField]
    private GameObject buttonList;

    [SerializeField]
    private OptionsController optionsPanel;

    // Start is called before the first frame update
    void Start()
    {
         this.isOpened = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            this.SetState(!this.isOpened);      
        }
    }

    private void SetState(bool open)
    {
        this.isOpened = open;
        this.grayOutPanel.SetActive(open);
        this.buttonList.SetActive(open);
        if (!open)
        {
            this.CloseSettingsWindow();
        }
    }

    public void SwitchEscapeMenu()
    {
        this.SetState(!this.isOpened);
    }

    public void ResumeGame()
    {
        this.SetState(open: false);
    }

    public void OpenSettings()
    {
        Debug.Log("Opening settings screen");
        this.buttonList.SetActive(false);
        this.optionsPanel.gameObject.SetActive(true);
    }

    public void ConcedeRound()
    {
        GameController.Singleton.CmdConcedRound();
        this.SetState(open: false);
    }

    public void ReturnToMainMenu()
    {
        if (GameController.Singleton != null && !GameController.Singleton.isServer)
        {
            //since Network manager hook doesnt handle scene change here, manually play transition before disconnecting
            GameObject transitioner = GameObject.FindWithTag("SceneTransitioner");
            if (transitioner != null)
                transitioner.GetComponent<SceneTransitioner>().FadeOut(() => NetworkManager.singleton.StopHost());

        } else
        {
            //server properly handles scene change transition
            MyNetworkManager.singleton.StopHostWithTransitionsOnAllClients();
        }        
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game should be closed");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    //for close button clicked on settings window
    public void ReturnToEscapeMenu()
    {
        this.buttonList.SetActive(true);
        this.CloseSettingsWindow();
    }

    //close window from any state
    //used for closing window normal and closing window when escape menu is exited
    private void CloseSettingsWindow()
    {
        this.optionsPanel.gameObject.SetActive(false);

    }
}
