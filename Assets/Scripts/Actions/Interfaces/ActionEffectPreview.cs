using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct ActionEffectPreview
{
    public readonly EffectOnCharacter[] effectOnCharacters;

    public ActionEffectPreview(EffectOnCharacter[] effectOnCharacters)
    {
        this.effectOnCharacters = effectOnCharacters;
    }
}
