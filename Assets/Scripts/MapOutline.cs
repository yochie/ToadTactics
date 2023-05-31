using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapOutline : MonoBehaviour
{
    public Map map;
    private Collider2D outlineCollider;


    //checks hexes in map overlapping this objects collider to destroy them
    public void DeleteHexesOutside()
    {
        outlineCollider = this.GetComponent<Collider2D>();
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        int collidingCount = Physics2D.OverlapCollider(this.outlineCollider, filter, results);

        for (int x = -map.xSize + 1; x < map.xSize; x++)
        {
            for (int y = -map.ySize + 1; y < map.ySize; y++)
            {
                Hex h = map.GetHex(x, y);
                Collider2D hCollider = h.GetComponent<Collider2D>();

                if (!results.Contains(hCollider))
                {
                    map.SetHex(x,y,null);
                    Destroy(h.gameObject);
                } else
                {
                    //coordinates hidden by default using canvas group alpha
                    //use that component in editor mode to display
                    TextMeshProUGUI coordLabel = Instantiate<TextMeshProUGUI>(map.cellLabelPrefab);
                    coordLabel.rectTransform.SetParent(map.coordCanvas.transform, false);
                    coordLabel.rectTransform.anchoredPosition =
                        new Vector2(h.transform.position.x, h.transform.position.y);
                    coordLabel.text = h.coordinates.ToStringOnLines();


                    //labels to display single number during navigation (range, etc)
                    TextMeshProUGUI numLabel = Instantiate<TextMeshProUGUI>(map.cellLabelPrefab);
                    numLabel.fontSize = 4;
                    numLabel.rectTransform.SetParent(map.labelsCanvas.transform, false);
                    numLabel.rectTransform.anchoredPosition =
                        new Vector2(h.transform.position.x, h.transform.position.y);
                    h.LabelRef = numLabel;

                }
            }
        }

        this.gameObject.SetActive(false);
    }
}
