using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundEndScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

    [SerializeField]
    private TextMeshProUGUI scoreLabel;

    [SerializeField]
    private TextMeshProUGUI continueButtonText;

    [SerializeField]
    private Button hideButton;

    [SerializeField]
    private TextMeshProUGUI hideButtonText;

    [SerializeField]
    private GameObject textContent;

    private bool hidden;

    private Color baseColor;

    private float baseAlpha;

    private void Awake()
    {
        this.hidden = false;
        this.baseColor = this.container.GetComponent<Image>().color;
        this.baseAlpha = this.baseColor.a;
    }

    public void ActivateMyself()
    {
        Debug.LogFormat("Activating round end screen {0}", this);
        int yourScore = GameController.Singleton.GetScore(GameController.Singleton.LocalPlayer.playerID);
        int opponentScore = GameController.Singleton.GetScore(GameController.Singleton.NonLocalPlayer.playerID);
        this.scoreLabel.text = string.Format("You : {0}\nOpponent : {1}", yourScore, opponentScore);
        this.container.SetActive(true);
        this.textContent.SetActive(true);
        this.hidden = false;
        this.hideButton.gameObject.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        if (GameController.Singleton.isServer)
            MyNetworkManager.singleton.StopHost();
        else
            MyNetworkManager.singleton.StopClient();
    }

    public void NextRound()
    {
        if (!GameController.Singleton.isServer)
            //TODO : make it so both players have to click button, not just host
            this.continueButtonText.text = "Waiting for host to proceed";
        else
            GameController.Singleton.CmdChangeToScene("EquipmentDraft");
    }

    public void ToggleHideScreen()
    {
        this.hidden = !this.hidden;
        this.container.GetComponent<Image>().color = Utility.SetAlpha(this.baseColor, this.hidden ? 0 : this.baseAlpha);
        this.textContent.SetActive(!this.hidden);
        this.hideButtonText.text = this.hidden ? "Return" : "View game";
    }
}
