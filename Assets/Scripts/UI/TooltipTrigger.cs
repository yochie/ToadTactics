using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject tooltipObject;

    [SerializeField]
    private Image highlight;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.tooltipObject.SetActive(true);
        if (highlight != null)
            highlight.color = Utility.SetAlpha(highlight.color, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.tooltipObject.SetActive(false);
        if (highlight != null)
            highlight.color = Utility.SetAlpha(highlight.color, 0f);
    }
}
