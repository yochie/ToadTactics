using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoundEndMessage : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

    [SerializeField]
    private TextMeshProUGUI scoreLabel;

    public void ActivateMyself()
    {
        Debug.LogFormat("Activating round end message {0}", this);
        int yourScore = GameController.Singleton.GetScore(GameController.Singleton.LocalPlayer.playerID);
        int opponentScore = GameController.Singleton.GetScore(GameController.Singleton.NonLocalPlayer.playerID);
        this.scoreLabel.text = string.Format("You : {0}\nOpponent : {1}", yourScore, opponentScore);
        this.container.SetActive(true);
    }
}
