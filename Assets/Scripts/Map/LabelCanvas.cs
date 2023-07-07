using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelCanvas : MonoBehaviour
{
    public static LabelCanvas instance;
    // Start is called before the first frame update
    void Awake()
    {
        LabelCanvas.instance = this;
    }
}

