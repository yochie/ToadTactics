using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectionFeedback : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI label;

    public void Display(string message)
    {
        this.gameObject.SetActive(true);
        this.label.text = message;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
