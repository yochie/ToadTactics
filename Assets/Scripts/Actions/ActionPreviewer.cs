using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewer : MonoBehaviour
{
    [SerializeField]
    private TurnOrderHUD turnOrderHUD;

    [SerializeField]
    private Map map;

    internal void PreviewActionEffect(ActionEffectPreview actionEffect)
    {
        foreach(EffectOnCharacter effect in actionEffect.effectOnCharacters)
        {
            int classID = effect.classID;
            PlayerCharacter character = GameController.Singleton.PlayerCharactersByID[classID];
            int currentLife = character.CurrentLife;
            int maxLife = character.CurrentStats.maxHealth;
            this.turnOrderHUD.DisplayDamagePreview(effect.classID, effect.minDamage, currentLife, maxLife);
        }
    }

    internal void RemoveActionPreview()
    {
        this.turnOrderHUD.HideAllDamagePreviews();
    }
}
