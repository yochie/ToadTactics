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
        NetworkManager.singleton.StopHost();
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game should be closed");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void CloseSettings()
    {
        this.buttonList.SetActive(true);
        this.optionsPanel.gameObject.SetActive(false);
    }
}
