using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundEndMessage : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    public void ActivateMyself()
    {
        Debug.LogFormat("Activating round end message {0}", this);
        this.content.SetActive(true);
    }
}
