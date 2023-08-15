using System.Collections.Generic;

public readonly struct TurnOrderSlotInitData
{
    readonly int classID;
    readonly Dictionary<int, string> buffIconsByID;

    public TurnOrderSlotInitData(int classID, Dictionary<int, string> buffIconsByID)
    {
        this.classID = classID;
        this.buffIconsByID = buffIconsByID;
    }
}