using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IBuffDataSO
{
    public string stringID { get; set; }

    public string UIName { get; set; }

    public bool IsPositive { get; set; }

    public DurationType DurationType { get; set; }

    public int TurnDuration{ get; set; }

    public Image Icon { get; set; }

    public string tooltipDescription { get; set; }
}