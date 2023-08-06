using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDGenerator : MonoBehaviour
{
    static private int currentID = 0;
    public static int GetNewID()
    {
        return currentID++;
    }
}
