using System;

public interface IMitigationEnhancer : IComparable<IMitigationEnhancer>
{
    public Hit MitigateHit(Hit hitToMitigate);
    public float PriorityOrder { get; set; }
}