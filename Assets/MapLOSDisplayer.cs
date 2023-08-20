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

    private void Start()
    {
        this.lineRenderer.positionCount = 2;
    }

    internal void DisplayLOS(Hex source, Hex destination, bool highlightPath)
    {
        this.fromPosition = source.transform.position;
        this.toPosition = destination.transform.position;

        this.lineRenderer.enabled = true;
        this.lineRenderer.SetPosition(0, fromPosition);
        this.lineRenderer.SetPosition(1, toPosition);
        float length = Vector2.Distance(fromPosition, toPosition);
        float horizontalFactor = 120f;
        float verticalFactor = 1.5f;
        this.lineRenderer.textureScale = new Vector2(length / horizontalFactor, verticalFactor);
    }

    internal void HideLOS()
    {
        this.lineRenderer.enabled = false;
    }
}
