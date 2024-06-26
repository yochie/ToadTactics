using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class Utility
{
    public static readonly string IPV4Regex = @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$";
    public static readonly int MAX_DISTANCE_ON_MAP = 50;
    public static readonly int NUM_PLAYERS = 2;
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
        if (damage == 0 || damageIterations <= 0 || damageType == DamageType.none)
            return "";

        string formattedDamageType = Utility.FormattedDamageType(damageType);

        return String.Format("{0} x {1} ({2})", damage == -1 ? "base" : damage, damageIterations, formattedDamageType);

    }

    internal static string FormattedDamageType(DamageType damageType)
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

        return formattedDamageType;
    }

    internal static Color SetAlpha(Color oldColor, float alpha)
    {
        Color newColor = oldColor;
        newColor.a = alpha;
        return newColor;

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

    internal static bool RollChance(float chanceForTrue)
    {
        if (UnityEngine.Random.Range(0f, 0.9999999f) < chanceForTrue)
            return true;
        else
            return false;
    }

    internal static int CalculateCritDamage(int damage, float critMultiplier)
    {
        return (int) Math.Round(damage * critMultiplier);
    }

    internal static List<T> GetAllEnumValues<T> ()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    private static System.Random rng = new System.Random();



    public static void Shuffle<T>(this IList<T> list)
    {
        List<T> newList = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //Used for rich text labelling
    public static string DamageTypeToColorName (DamageType dmgType)
    {
        string damageTypeTextColor;
        switch (dmgType)
        {
            case DamageType.magic:
                damageTypeTextColor = "blue";
                break;
            case DamageType.healing:
                damageTypeTextColor = "green";
                break;
            default:
                damageTypeTextColor = "white";
                break;
        }
        return damageTypeTextColor;
    }
}
