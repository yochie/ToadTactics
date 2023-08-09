using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    internal static Color GrayOutColor(Color oldColor, bool state)
    {
        Color unGrayedOutColor = oldColor;
        Color grayedOutColor = oldColor;
        unGrayedOutColor.a = 1f;
        grayedOutColor.a = 0.2f;
        return state ? grayedOutColor : unGrayedOutColor;
    }

    internal static int ApplyKingLifeBuff(int maxHealth)
    {
        return maxHealth + 100;
    }

    internal static bool RollCrit(float critChance)
    {
        if (UnityEngine.Random.Range(0f, 0.999f) < critChance)
            return true;
        else
            return false;
    }

    internal static int CalculateCritDamage(int damage, float critMultiplier)
    {
        return Convert.ToInt32(damage * critMultiplier);
    }

    internal static List<T> GetAllEnumValues<T> ()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }
}
