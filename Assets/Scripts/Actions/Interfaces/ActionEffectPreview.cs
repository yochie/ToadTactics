using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public readonly struct ActionEffectPreview
{
    public readonly EffectOnCharacter[] effectOnCharacters;

    public static ActionEffectPreview None()
    {
        return new ActionEffectPreview(new EffectOnCharacter[] { });
    }

    public ActionEffectPreview(EffectOnCharacter[] effectOnCharacters)
    {
        this.effectOnCharacters = effectOnCharacters;
    }

    public ActionEffectPreview AddEffect(EffectOnCharacter effectToAdd)
    {
        if (effectToAdd.classID == -1)
            return this;

        bool characterWasAlreadyAffected = false;
        int previousIndex = 0;
        foreach (EffectOnCharacter effect in this.effectOnCharacters)
        {
            if (effect.classID == effectToAdd.classID)
            {
                characterWasAlreadyAffected = true;               
                break;
            }
            previousIndex++;
        }

        EffectOnCharacter[] newEffects;
        if (characterWasAlreadyAffected)
        {
            newEffects = this.effectOnCharacters.ToArray();
            EffectOnCharacter previousEffect = newEffects[previousIndex];
            if (previousEffect.classID == effectToAdd.classID)
            {
                newEffects[previousIndex] = previousEffect.Add(effectToAdd);
            }

        } else {
            newEffects = new EffectOnCharacter[this.effectOnCharacters.Length + 1];
            this.effectOnCharacters.CopyTo(newEffects, 0);
            newEffects[newEffects.Length - 1] = effectToAdd;
        }

        return new ActionEffectPreview(newEffects);
    }

    internal ActionEffectPreview MergeWithPreview(ActionEffectPreview toMerge)
    {
        ActionEffectPreview newPreview = this;
        foreach(EffectOnCharacter effect in toMerge.effectOnCharacters)
        {
            newPreview = newPreview.AddEffect(effect);
        }
        return newPreview;
    }
}
