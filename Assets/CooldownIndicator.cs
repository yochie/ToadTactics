using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CooldownIndicator : MonoBehaviour
{
    [SerializeField]
    private GameObject icon;

    [SerializeField]
    private TextMeshProUGUI cooldownText;

    [SerializeField]
    private GameObject cooldownContainer;

    [SerializeField]
    private TextMeshProUGUI usesRemainingText;

    internal void SetCooldown(string currentCooldown)
    {
        if(currentCooldown == "")
        {
            this.ActivateCooldownIndication(false);
            return;
        }
        this.ActivateCooldownIndication(true);
        this.cooldownText.text = currentCooldown;
    }

    internal void SetUsesCount(string currentRemainingUses)
    {
        if (currentRemainingUses == "")
        {
            this.ActivateUsesIndication(false);
            return;
        }

        this.ActivateUsesIndication(true);
        this.usesRemainingText.text = currentRemainingUses;
    }

    private void ActivateCooldownIndication(bool state)
    {
        this.cooldownContainer.SetActive(state);
    }

    private void ActivateUsesIndication(bool state)
    {
        this.usesRemainingText.gameObject.SetActive(state);
    }
}
