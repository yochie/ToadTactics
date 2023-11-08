using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealPreview : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private bool displayed = false;

    //portion of remaining life should be in range [0, 1]
    public void Display(bool state, float portionOfLifeAfterHeal)
    {
        this.displayed = state;

        if (state)
            this.transform.localScale = new Vector3(portionOfLifeAfterHeal, 1, 1);
        else
            this.image.color = Utility.SetAlpha(this.image.color, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (displayed)
            this.image.color = Utility.SetAlpha(this.image.color, Mathf.PingPong(Time.time, 1f));
    }
}
