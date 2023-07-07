using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordCanvas : MonoBehaviour
{
    public static CoordCanvas instance;
    // Start is called before the first frame update
    void Awake()
    {
        CoordCanvas.instance = this;
    }
}
