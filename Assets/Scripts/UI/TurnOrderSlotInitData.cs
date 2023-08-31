using Mirror;
using System.Collections.Generic;
using System.Linq;

public readonly struct TurnOrderSlotInitData
{
    public readonly int classID;
    public readonly bool isAKing;
    public readonly bool itsHisTurn;
    public readonly int maxHealth;    

    //These two have to correspond implicitely via index because we cant send Dictionary over network
    //Implementing serialization manually could be a more reliable solution but I think its essentially equivalent...    
    public readonly List<int> orderedBuffIDs;
    public readonly List<string> orderedBuffDataIDs;

    public TurnOrderSlotInitData(int classID, bool isAKing, bool itsHisTurn, int maxHealth, Dictionary<int, string> buffDataIDByUniqueID)
    {
        this.classID = classID;
        this.isAKing = isAKing;
        this.itsHisTurn = itsHisTurn;
        this.maxHealth = maxHealth;
        this.orderedBuffIDs = buffDataIDByUniqueID.Keys.ToList();
        this.orderedBuffDataIDs = buffDataIDByUniqueID.Values.ToList();
    }
}

//public static class TurnOrderSlotInitDataWriter 
//{
//    public static void WriteTurnOrderSlotInitData(this NetworkWriter writer, TurnOrderSlotInitData slotData)
//    {
//        writer.WriteInt(slotData.ClassID);
//        writer.WriteString(slotData.BuffIconsByID.ToString());
//        writer.WriteString(slotData.BuffIconsByID.Values.ToString());
//    }

//    public static TurnOrderSlotInitData ReadTurnOrderSlotInitData(this NetworkReader reader)
//    {
//        Dictionary<int, string> = new().fro
//        return new TurnOrderSlotInitData(reader.ReadInt());
//    }
//}