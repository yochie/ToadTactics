using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartZone : MonoBehaviour
{
    [SerializeField]
    private Collider2D outlineCollider;
    [SerializeField]
    private int playerIndex;


    //checks hexes in map overlapping this objects collider to destroy them
    public void SetStartingZone(Dictionary<Vector2Int, Hex> grid, int xSize, int ySize)
    {        
        List<Collider2D> results = new();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);
        //Debug.Log(collidingCount);

        for (int x = -xSize + 1; x < xSize; x++)
        {
            for (int y = -ySize + 1; y < ySize; y++)
            {
                Hex h = Map.GetHex(grid, x, y);
                if (h == null)
                {
                    continue;
                }
                Collider2D hCollider = h.GetComponent<Collider2D>();

                if (results.Contains(hCollider))
                {
                    h.isStartingZone = true;
                    h.startZoneForPlayerIndex = this.playerIndex;
                }
            }
        }
        this.gameObject.SetActive(false);
    }
}
