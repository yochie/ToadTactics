using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameController : MonoBehaviour
{

    public Map map;

    // Start is called before the first frame update
    void Start()
    {
        this.map.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
