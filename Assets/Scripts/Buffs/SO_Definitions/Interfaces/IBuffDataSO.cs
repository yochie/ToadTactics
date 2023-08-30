using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IBuffDataSO
{
    //Set in implementing class definitions
    [field: SerializeField]
    public string BuffTypeID { get; set; }

    [field: SerializeField]
    public string UIName { get; set; }

    [field: SerializeField]
    public bool IsPositive { get; set; }

    [field: SerializeField]
    public DurationType DurationType { get; set; }

    [field: SerializeField]
    public int TurnDuration{ get; set; }

    [field: SerializeField]
    public Image Icon { get; set; }

    [field: SerializeField]
    public string tooltipDescription { get; set; }
}