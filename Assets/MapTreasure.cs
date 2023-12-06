using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTreasure : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer render;

    [ClientRpc]
    public void RpcSetVisible(bool val)
    {
        AnimationSystem.Singleton.Queue(this.SetVisibleCoroutine(val));
    }

    private IEnumerator SetVisibleCoroutine(bool val)
    {
        this.render.enabled = val;
        yield break;
    }
}
