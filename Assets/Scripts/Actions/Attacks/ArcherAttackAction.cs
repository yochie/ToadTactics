using Mirror;
using System.Collections.Generic;

internal class ArcherAttackAction : DefaultAttackAction, IPrintableStats
{    
    public Dictionary<string, string> GetStatsDictionary()
    {
        var toReturn = new Dictionary<string, string>();
        toReturn.Add("Attack area", "line to target");
        return toReturn;
    }
}