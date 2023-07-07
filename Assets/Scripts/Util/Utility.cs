using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
    internal static bool DictContainsValue(SyncIDictionary<float, int> dict, int item)
    {
        foreach (float key in dict.Keys)
        {
            if(dict[key] == item)
            {
                return true;
            }
        }

        return false;
    }

    internal static Color SetHighlight(Color oldColor, bool state)
    {
        Color highlightOff = oldColor;
        Color highlightOn = oldColor;
        highlightOn.a = 0.5f;
        highlightOff.a = 0f;
        return state ? highlightOn : highlightOff;

    }
}
