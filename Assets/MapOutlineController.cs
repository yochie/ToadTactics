using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapOutlineController : MonoBehaviour
{
    public Map map;
    private Collider2D outlineCollider;

    public void deleteHexesOutside()
    {
        outlineCollider = this.GetComponent<Collider2D>();
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);
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
                }
                else
                {
                    Destroy(h.gameObject);
                }
            }
        }
    }
}
