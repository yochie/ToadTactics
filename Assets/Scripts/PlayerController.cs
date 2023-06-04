using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("A new player has entered the lobby.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("A player has left the lobby.");

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
