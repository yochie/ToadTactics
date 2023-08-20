using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLOSDisplayer : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;

    private Vector2 fromPosition;
    private Vector2 toPosition;
    private List<Hex> higlightedPath;
    private bool displaying;

    internal void DisplayLOS(Hex source, Hex destination, bool highlightPath)
    {
        this.displaying = true;
        this.fromPosition = source.transform.position;
        this.toPosition = destination.transform.position;

    }

    internal void HideLOS()
    {
        this.displaying = false;
    }


    private void Update()
    {
        if (this.displaying)
        {
        }
    }
}
