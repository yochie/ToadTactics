using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSystem : MonoBehaviour
{
    public static AnimationSystem Singleton { get; private set; }
    private CoroutineQueue queue;


    public void Awake()
    {
        AnimationSystem.Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a coroutine queue that can run up to two coroutines at once
        this.queue = new CoroutineQueue(1, StartCoroutine);
    }

    public void Queue(IEnumerator coroutine)
    {
        this.queue.Run(coroutine);
    }
}
