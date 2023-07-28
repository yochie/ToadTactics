using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RoundEndScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

    [SerializeField]
    private TextMeshProUGUI scoreLabel;

    [SerializeField]
    private TextMeshProUGUI buttonText;

    public void ActivateMyself()
    {
        Debug.LogFormat("Activating round end screen {0}", this);
        int yourScore = GameController.Singleton.GetScore(GameController.Singleton.LocalPlayer.playerID);
        int opponentScore = GameController.Singleton.GetScore(GameController.Singleton.NonLocalPlayer.playerID);
        this.scoreLabel.text = string.Format("You : {0}\nOpponent : {1}", yourScore, opponentScore);
        this.container.SetActive(true);
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
            this.buttonText.text = "Waiting for host to proceed";
        else
            GameController.Singleton.CmdChangeToScene("EquipmentDraft");
    }
}
