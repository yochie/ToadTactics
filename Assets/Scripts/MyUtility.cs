using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyUtility
{
    public static Color hexHoverColor = Color.cyan;
    public static Color hexBaseColor = Color.white;
    internal static Color hexSelectedColor = Color.green;

    public Vector3 subtractVector3 (Vector3 v1, Vector3 v2)
    {
        return v2 - v1;
    }

    public static float HexDistance(Hex h1, Hex h2)
    {
        HexCoordinates hc1 = h1.coordinates;
        HexCoordinates hc2 = h2.coordinates;

        Vector3 diff = new Vector3(hc1.Q, hc1.R, hc1.S) - new Vector3(hc2.Q, hc2.R, hc2.S);

        return (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z)) / 2f;
    }
}
