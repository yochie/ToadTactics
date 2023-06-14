using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    internal static bool ContainsValue(SyncIDictionary<float, int> dict, int item)
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
}
