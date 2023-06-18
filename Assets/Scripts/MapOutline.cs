using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class MapOutline : NetworkBehaviour
{
    public Map map;
    private Collider2D outlineCollider;


    //checks hexes in map overlapping this objects collider to destroy them
    [Server]
    public void DeleteHexesOutside()
    {
        //Debug.Log("Deleting hexes outside outline");
        outlineCollider = this.GetComponent<Collider2D>();
        List<Collider2D> results = new();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);

        for (int x = -map.xSize + 1; x < map.xSize; x++)
        {
            for (int y = -map.ySize + 1; y < map.ySize; y++)
            {
                Hex h = map.GetHex(x, y);
                //Debug.Log(map + " " + x + " " + y);
                //Debug.Log(map.GetHex(x, y));

                Collider2D hCollider = h.GetComponent<Collider2D>();

                if (!results.Contains(hCollider))
                {
                    //Debug.Log("Deleting hex");
                    map.DeleteHex(x, y);
                }
            }
        }

        this.gameObject.SetActive(false);
    }
}
