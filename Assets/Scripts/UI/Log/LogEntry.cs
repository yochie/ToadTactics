using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LogEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageText;

    public void SetText(string message)
    {
        this.messageText.text = message;
    }
}
