using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipContent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private StatsTable statsTable;

    public void SetTitle(string text)
    {
        this.title.text = text;
    }

    public void FillWithDictionary(Dictionary<string, string> fillWith)
    {
        if (fillWith.Count > 0)
            this.statsTable.gameObject.SetActive(true);
        else
        {
            this.statsTable.gameObject.SetActive(false);
        }

        this.statsTable.RenderFromDictionary(fillWith);
    }
}
