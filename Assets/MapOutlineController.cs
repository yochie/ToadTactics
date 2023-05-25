using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapOutlineController : MonoBehaviour
{
    private bool hasRun = false;
    public Map map;
    private Collider2D outlineCollider;

    // Start is called before the first frame update
    void Start()
    {
        outlineCollider = this.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasRun)
        {
            List<Collider2D> results = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D().NoFilter();
            int collidingCount = Physics2D.OverlapCollider(outlineCollider, filter, results);
            Debug.Log(collidingCount);

            for (int x = -map.xSize + 1; x < map.xSize; x++)
            {
                for (int y = -map.ySize + 1; y < map.ySize; y++)
                {
                    Hex h = map.GetHex(x, y);
                    Collider2D hCollider = h.GetComponent<Collider2D>();

                    if (results.Contains(hCollider))
                    {
                        Debug.Log("get in here");
                    } else
                    {
                        Destroy(h.gameObject);
                    }
                }
            }
            hasRun = true;
        }
    }
}
