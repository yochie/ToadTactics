using Mirror;
using System.Collections.Generic;

internal class WarriorAttackAction : DefaultAttackAction, IPrintableStats
{
    public Dictionary<string, string> GetStatsDictionary()
    {
        var toReturn = new Dictionary<string, string>();
        toReturn.Add("Attack area", "arc");
        return toReturn;
    }
}