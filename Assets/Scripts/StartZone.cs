using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartZone : MonoBehaviour
{
    public Map map;
    private Collider2D outlineCollider;
    public int playerIndex;


    //checks hexes in map overlapping this objects collider to destroy them
    public void SetStartingZone()
    {
        outlineCollider = this.GetComponent<Collider2D>();
        List<Collider2D> results = new();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);
        //Debug.Log(collidingCount);

        for (int x = -map.xSize + 1; x < map.xSize; x++)
        {
            for (int y = -map.ySize + 1; y < map.ySize; y++)
            {
                Hex h = map.GetHex(x, y);
                if (h == null)
                {
                    continue;
                }
                Collider2D hCollider = h.GetComponent<Collider2D>();

                if (results.Contains(hCollider))
                {
                    h.isStartingZone = true;
                    h.baseColor = map.HEX_START_BASE_COLOR;
                }
            }
        }
        this.gameObject.SetActive(false);
    }
}
