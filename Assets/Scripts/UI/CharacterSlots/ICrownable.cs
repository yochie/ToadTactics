using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ICrownable 
{
    public Image CrownImage { get; set; }

    public void DisplayCrown(bool state);
}
