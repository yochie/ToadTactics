using System.Collections.Generic;

public interface IBuff
{
    //Set in implementing class definitions
    //TODO: remove this. pretty sure its unused since we directly refer to buff Type where applicable (ability actions, passives)
    public string BuffTypeID { get; }

    public string UIName { get; }

    //Set at runtime
    public int UniqueID { get; set; }

    public List<int> AffectedCharacterIDs { get; set; }

    public bool IsPositive { get; }
}