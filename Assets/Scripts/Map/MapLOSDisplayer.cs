using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLOSDisplayer : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;

    private List<Hex> highlightedPath = new();

    private void Start()
    {
        this.lineRenderer.positionCount = 2;
    }

    internal void DisplayLOS(Hex source, Hex destination, bool highlightPath)
    {
        Vector2 fromPosition = source.transform.position;
        Vector2 toPosition = destination.transform.position;

        this.lineRenderer.enabled = true;
        this.lineRenderer.SetPosition(0, fromPosition);
        this.lineRenderer.SetPosition(1, toPosition);
        float length = Vector2.Distance(fromPosition, toPosition);
        float horizontalFactor = 120f;
        float verticalFactor = 1.5f;
        this.lineRenderer.textureScale = new Vector2(length / horizontalFactor, verticalFactor);

        if (highlightPath)
        {
            this.highlightedPath = MapPathfinder.HexesOnLine(source, destination, excludeStart: true, excludeDestination: true);
            foreach(Hex hex in this.highlightedPath)
            {
                hex.drawer.AbilityHover(true);
            }

        }
    }

    internal void HideLOS()
    {
        this.lineRenderer.enabled = false;
        foreach(Hex hex in this.highlightedPath)
        {
            hex.drawer.AbilityHover(false);
        }
        this.highlightedPath.Clear();
    }
}
