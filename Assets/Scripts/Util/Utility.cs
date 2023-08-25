using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class Utility
{

    public static readonly int MAX_DISTANCE_ON_MAP = 50;
    internal static Color DEFAULT_BUTTON_COLOR = new(149/255f, 140/255f, 61/255f);

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

    internal static string DamageStatsToString(int damage, int damageIterations, DamageType damageType)
    {
        string formattedDamageType;
        switch (damageType)
        {
            case DamageType.physical:
                formattedDamageType = "phys";
                break;
            case DamageType.healing:
                formattedDamageType = "heal";
                break;
            case DamageType.magic:
                formattedDamageType = "magic";
                break;
            default:
                formattedDamageType = damageType.ToString();
                break;
        }
        return String.Format("{0} x {1} ({2})", damage, damageIterations, formattedDamageType);


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
