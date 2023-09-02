using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject tooltipObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.tooltipObject.SetActive(false);
    }
}
