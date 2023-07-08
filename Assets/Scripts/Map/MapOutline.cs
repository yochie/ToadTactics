using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

[RequireComponent(typeof(Collider2D))]
public class MapOutline : MonoBehaviour
{
    [SerializeField]
    private Collider2D outlineCollider;

    //checks hexes in map overlapping this objects collider to destroy them
    [Server]
    public void DeleteHexesOutside(Dictionary<Vector2Int, Hex> grid, int xSize, int ySize)
    {
        //Debug.Log("Deleting hexes outside outline");
        List<Collider2D> results = new();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);

        for (int x = -xSize + 1; x < xSize; x++)
        {
            for (int y = -ySize + 1; y < ySize; y++)
            {
                Hex h = Map.GetHex(grid, x, y);
                //Debug.Log(map + " " + x + " " + y);
                //Debug.Log(map.GetHex(x, y));

                Collider2D hCollider = h.GetComponent<Collider2D>();

                if (!results.Contains(hCollider))
                {
                    //Debug.Log("Deleting hex");
                    Map.DeleteHex(grid, x, y);
                }
            }
        }

        this.gameObject.SetActive(false);
    }
}
